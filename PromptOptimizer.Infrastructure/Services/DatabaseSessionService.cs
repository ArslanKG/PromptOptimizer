using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public DatabaseSessionService(AppDbContext context, ILogger<DatabaseSessionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ConversationSession> CreateSessionAsync(int? userId = null)
        {
            int validUserId;

            if (userId.HasValue && userId.Value > 0)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId.Value);
                if (!userExists)
                {
                    _logger.LogWarning("User with ID {UserId} not found", userId.Value);
                    var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.IsAdmin);
                    validUserId = adminUser?.Id ?? throw new InvalidOperationException("No admin user found");
                }
                else
                {
                    validUserId = userId.Value;
                }
            }
            else
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.IsAdmin);
                validUserId = adminUser?.Id ?? throw new InvalidOperationException("No admin user found");
            }

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

            var messages = new List<ConversationMessage>();
            if (!string.IsNullOrEmpty(sessionEntity.MessagesJson))
            {
                try
                {
                    messages = JsonSerializer.Deserialize<List<ConversationMessage>>(
                        sessionEntity.MessagesJson, JsonOptions) ?? new List<ConversationMessage>();
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing messages for session {SessionId}", sessionId);
                }
            }

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

        public async Task<ConversationSession> AddMessageAsync(string sessionId, ConversationMessage message)
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session {sessionId} not found");
            }

            session.Messages.Add(message);

            session.MessagesJson = JsonSerializer.Serialize(session.Messages, JsonOptions);
            session.MessageCount = session.Messages.Count;
            session.LastActivityAt = DateTime.UtcNow;

            if ((string.IsNullOrEmpty(session.Title) || session.Title == "New Session") && message.Role == "user")
            {
                session.Title = message.Content.Length > 50
                    ? $"{message.Content[..50]}..."
                    : message.Content;
            }

            var sessionEntity = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (sessionEntity != null)
            {
                sessionEntity.MessagesJson = session.MessagesJson;
                sessionEntity.MessageCount = session.MessageCount;
                sessionEntity.LastActivityAt = session.LastActivityAt;
                sessionEntity.Title = session.Title;

                await _context.SaveChangesAsync();
            }

            return session;
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

        public async Task<List<ConversationMessage>> GetConversationContextAsync(string sessionId, int? windowSize = null)
        {
            var session = await GetSessionAsync(sessionId);

            if (session == null || session.Messages.Count == 0)
            {
                return new List<ConversationMessage>();
            }

            var size = windowSize ?? 10;
            return session.Messages
                .OrderBy(m => m.Timestamp)
                .TakeLast(size)
                .ToList();
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
            var sessionEntity = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
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
            return await _context.Sessions.AnyAsync(s => s.SessionId == sessionId && s.IsActive);
        }
    }
}