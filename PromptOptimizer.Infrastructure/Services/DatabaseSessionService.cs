// PromptOptimizer.Infrastructure/Services/DatabaseSessionService.cs
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;
using PromptOptimizer.Infrastructure.Data;

namespace PromptOptimizer.Infrastructure.Services
{
    public class DatabaseSessionService : ISessionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseSessionService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        public DatabaseSessionService(AppDbContext context, ILogger<DatabaseSessionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ConversationSession> CreateSessionAsync(int? userId = null)
        {
            var validUserId = await GetValidUserIdAsync(userId);

            var session = new ConversationSession
            {
                SessionId = Guid.NewGuid().ToString(),
                UserId = validUserId,
                Title = "New Session",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsActive = true,
                Messages = new List<ConversationMessage>(),
                MessageCount = 0,
                MaxMessages = 100,
                MessagesJson = "[]"
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created session {SessionId} for user {UserId}", session.SessionId, validUserId);
            return session;
        }

        public async Task<ConversationSession?> GetSessionAsync(string sessionId)
        {
            var sessionEntity = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (sessionEntity == null) return null;

            return MapEntityToSession(sessionEntity);
        }

        public async Task<ConversationSession> AddMessageAsync(string sessionId, ConversationMessage message)
        {
            try
            {
                _logger.LogDebug("Adding message to session {SessionId}: {Role} - {ContentPreview}",
                    sessionId, message.Role, message.Content[..Math.Min(50, message.Content.Length)]);

                var sessionEntity = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

                if (sessionEntity == null)
                {
                    throw new InvalidOperationException($"Session {sessionId} not found");
                }

                var existingMessages = DeserializeMessages(sessionEntity.MessagesJson);
                existingMessages.Add(message);

                sessionEntity.MessagesJson = JsonSerializer.Serialize(existingMessages, JsonOptions);
                sessionEntity.MessageCount = existingMessages.Count;
                sessionEntity.LastActivityAt = DateTime.UtcNow;

                if (ShouldUpdateTitle(sessionEntity.Title, message))
                {
                    sessionEntity.Title = GenerateSessionTitle(message.Content);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Saved message to session {SessionId}. Total messages: {Count}",
                    sessionId, existingMessages.Count);

                return MapEntityToSession(sessionEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add message to session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task AddMessagesAsync(string sessionId, List<ConversationMessage> messages)
        {
            try
            {
                var sessionEntity = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

                if (sessionEntity == null)
                {
                    throw new InvalidOperationException($"Session {sessionId} not found");
                }

                var existingMessages = DeserializeMessages(sessionEntity.MessagesJson);
                existingMessages.AddRange(messages);

                sessionEntity.MessagesJson = JsonSerializer.Serialize(existingMessages, JsonOptions);
                sessionEntity.MessageCount = existingMessages.Count;
                sessionEntity.LastActivityAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Batch saved {Count} messages to session {SessionId}",
                    messages.Count, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to batch add messages to session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task AddMessageAsync(string sessionId, string role, string content)
        {
            var message = new ConversationMessage
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            await AddMessageAsync(sessionId, message);
        }

        // 🔥 KEY FIX: Proper conversation context retrieval
        public async Task<List<ConversationMessage>> GetConversationContextAsync(string sessionId, int? windowSize = null)
        {
            try
            {
                _logger.LogDebug("Getting conversation context for session {SessionId}, window: {WindowSize}",
                    sessionId, windowSize);

                var sessionEntity = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

                if (sessionEntity == null)
                {
                    _logger.LogWarning("Session {SessionId} not found", sessionId);
                    return new List<ConversationMessage>();
                }

                _logger.LogDebug("Session found - MessageCount: {Count}, JSON length: {JsonLength}",
                    sessionEntity.MessageCount, sessionEntity.MessagesJson?.Length ?? 0);

                var allMessages = DeserializeMessages(sessionEntity.MessagesJson);

                if (allMessages.Count == 0)
                {
                    _logger.LogDebug("No messages found in session {SessionId}", sessionId);
                    return new List<ConversationMessage>();
                }

                var windowSizeToUse = windowSize ?? 10;
                var recentMessages = allMessages
                    .OrderBy(m => m.Timestamp)
                    .TakeLast(windowSizeToUse)
                    .ToList();

                _logger.LogInformation("Retrieved {RecentCount} of {TotalCount} messages for context (session {SessionId})",
                    recentMessages.Count, allMessages.Count, sessionId);

                return recentMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation context for session {SessionId}", sessionId);
                return new List<ConversationMessage>();
            }
        }

        public async Task<ConversationHistoryResponse> GetConversationHistoryAsync(string sessionId, int? limit = null)
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null)
            {
                return new ConversationHistoryResponse { SessionId = sessionId };
            }

            var messages = limit.HasValue
                ? session.Messages.OrderBy(m => m.Timestamp).TakeLast(limit.Value).ToList()
                : session.Messages.OrderBy(m => m.Timestamp).ToList();

            return new ConversationHistoryResponse
            {
                SessionId = sessionId,
                Messages = messages,
                TotalMessages = session.MessageCount,
                LastActivityAt = session.LastActivityAt
            };
        }

        public async Task<bool> ClearSessionAsync(string sessionId)
        {
            var sessionEntity = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (sessionEntity != null)
            {
                sessionEntity.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleared session {SessionId}", sessionId);
                return true;
            }

            return false;
        }

        public async Task<List<SessionResponse>> GetActiveSessionsAsync(int? userId = null)
        {
            var query = _context.Sessions.Where(s => s.IsActive);

            if (userId.HasValue)
            {
                query = query.Where(s => s.UserId == userId.Value);
            }

            var sessions = await query
                .OrderByDescending(s => s.LastActivityAt)
                .Select(s => new SessionResponse
                {
                    SessionId = s.SessionId,
                    CreatedAt = s.CreatedAt,
                    MessageCount = s.MessageCount,
                    IsActive = s.IsActive,
                    LastActivityAt = s.LastActivityAt,
                    Title = s.Title ?? string.Empty
                })
                .ToListAsync();

            return sessions;
        }

        public async Task<bool> SessionExistsAsync(string sessionId)
        {
            return await _context.Sessions
                .AnyAsync(s => s.SessionId == sessionId && s.IsActive);
        }

        public async Task UpdateSessionTitleAsync(string sessionId, string title)
        {
            var sessionEntity = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (sessionEntity != null)
            {
                sessionEntity.Title = title;
                await _context.SaveChangesAsync();
                _logger.LogDebug("Updated session {SessionId} title to '{Title}'", sessionId, title);
            }
        }

        private async Task<int> GetValidUserIdAsync(int? userId)
        {
            if (userId.HasValue && userId.Value > 0)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId.Value && u.IsActive);
                if (userExists)
                {
                    _logger.LogDebug("Using provided user ID: {UserId}", userId.Value);
                    return userId.Value;
                }
                
                _logger.LogWarning("Provided user ID {UserId} not found or inactive", userId.Value);
                throw new InvalidOperationException($"User with ID {userId.Value} not found or inactive");
            }

            // For anonymous sessions, try to use a system/default user
            var systemUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "system" && u.IsActive);
            if (systemUser != null)
            {
                _logger.LogDebug("Using system user for anonymous session");
                return systemUser.Id;
            }

            // Last resort: use admin user but log it as a warning
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.IsAdmin && u.IsActive);
            if (adminUser != null)
            {
                _logger.LogWarning("No user ID provided and no system user found. Using admin user for session.");
                return adminUser.Id;
            }

            throw new InvalidOperationException("No valid user found. Please provide a valid user ID or ensure system/admin users exist.");
        }

        private ConversationSession MapEntityToSession(ConversationSession sessionEntity)
        {
            var messages = DeserializeMessages(sessionEntity.MessagesJson);

            return new ConversationSession
            {
                SessionId = sessionEntity.SessionId,
                UserId = sessionEntity.UserId,
                Title = sessionEntity.Title,
                Messages = messages,
                CreatedAt = sessionEntity.CreatedAt,
                LastActivityAt = sessionEntity.LastActivityAt,
                IsActive = sessionEntity.IsActive,
                MessageCount = sessionEntity.MessageCount,
                MaxMessages = sessionEntity.MaxMessages,
                MessagesJson = sessionEntity.MessagesJson
            };
        }

        private List<ConversationMessage> DeserializeMessages(string messagesJson)
        {
            if (string.IsNullOrEmpty(messagesJson) || messagesJson == "[]")
            {
                return new List<ConversationMessage>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<ConversationMessage>>(messagesJson, JsonOptions)
                       ?? new List<ConversationMessage>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing messages JSON: {Json}", messagesJson);
                return new List<ConversationMessage>();
            }
        }

        private bool ShouldUpdateTitle(string? currentTitle, ConversationMessage? message)
        {
            return string.IsNullOrEmpty(currentTitle) || currentTitle == "New Session";
        }

        private string GenerateSessionTitle(string content)
        {
            return content.Length > 50 ? $"{content[..47]}..." : content;
        }
    }
}