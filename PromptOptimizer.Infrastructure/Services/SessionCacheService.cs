using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Helpers;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Infrastructure.Services
{
    public class SessionCacheService : ISessionCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISessionService _sessionService;
        private readonly ILogger<SessionCacheService> _logger;
        private readonly AppDbContext _context;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        // JSON serialization ayarları - DatabaseSessionService ile aynı
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SessionCacheService(
            IMemoryCache memoryCache,
            AppDbContext context,
            ISessionService sessionService,
            ILogger<SessionCacheService> logger)
        {
            _memoryCache = memoryCache;
            _sessionService = sessionService;
            _logger = logger;
            _context = context;
        }

        public async Task<List<ConversationMessage>> GetSessionMessagesAsync(string sessionId)
        {
            var cacheKey = $"session_messages_{sessionId}";

            if (_memoryCache.TryGetValue(cacheKey, out List<ConversationMessage>? cachedMessages))
            {
                _logger.LogDebug("Retrieved {Count} messages from cache for session {SessionId}",
                    cachedMessages?.Count ?? 0, sessionId);
                return cachedMessages ?? new List<ConversationMessage>();
            }

            var session = await _sessionService.GetSessionAsync(sessionId);
            var messages = session?.Messages ?? new List<ConversationMessage>();

            _memoryCache.Set(cacheKey, messages, _cacheExpiration);

            _logger.LogDebug("Loaded {Count} messages from database and cached for session {SessionId}",
                messages.Count, sessionId);

            return messages;
        }

        public async Task AddMessageToSessionAsync(string sessionId, ConversationMessage message)
        {
            var messages = await GetSessionMessagesAsync(sessionId);
            messages.Add(message);

            var cacheKey = $"session_messages_{sessionId}";
            _memoryCache.Set(cacheKey, messages, _cacheExpiration);

            _logger.LogDebug("Added {Role} message to cache for session {SessionId}. Total cached messages: {Count}",
                message.Role, sessionId, messages.Count);

            if (messages.Count % 5 == 0 && messages.Count > 0)
            {
                var lastMessage = messages.LastOrDefault();
                if (lastMessage?.Role == "assistant")
                {
                    _logger.LogInformation("Conversation pair completed, flushing session {SessionId} to database", sessionId);
                    await FlushSessionToDatabaseAsync(sessionId);
                }
            }
        }

        public async Task FlushSessionToDatabaseAsync(string sessionId)
        {
            var messages = await GetSessionMessagesAsync(sessionId);
            if (!messages.Any()) return;

            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session == null) return;

            session.Messages = messages;
            session.MessagesJson = JsonSerializer.Serialize(messages, JsonOptions);
            session.MessageCount = messages.Count;
            session.LastActivityAt = DateTime.UtcNow;

            // Title auto-generation
            if (string.IsNullOrEmpty(session.Title) || session.Title == "New Session")
            {
                var firstUserMessage = messages.FirstOrDefault(m => m.Role == "user");
                if (firstUserMessage != null)
                {
                    session.Title = firstUserMessage.Content.Length > 50
                        ? $"{firstUserMessage.Content[..50]}..."
                        : firstUserMessage.Content;
                }
            }

            await UpdateSessionInDatabase(session);

            _logger.LogInformation("Flushed {Count} messages to database for session {SessionId}",
                messages.Count, sessionId);
        }

        public async Task<List<ChatMessage>> GetOptimizedContextAsync(string sessionId, int maxTokens = 4000)
        {
            var messages = await GetSessionMessagesAsync(sessionId);
            if (!messages.Any()) return new List<ChatMessage>();

            // Son 10 mesajı al
            var recentMessages = messages
                .OrderBy(m => m.Timestamp)
                .TakeLast(10)
                .ToList();

            var optimizedMessages = new List<ChatMessage>();
            var currentTokens = 0;

            // Geriye doğru git ve token limitine kadar ekle
            for (int i = recentMessages.Count - 1; i >= 0; i--)
            {
                var message = recentMessages[i];
                var messageTokens = TokenCounter.EstimateMessageTokens(message);

                if (currentTokens + messageTokens > maxTokens)
                {
                    if (i > 0)
                    {
                        var oldMessages = recentMessages.Take(i + 1).ToList();
                        var summary = TokenCounter.SummarizeOldMessages(oldMessages);

                        if (!string.IsNullOrEmpty(summary))
                        {
                            optimizedMessages.Insert(0, new ChatMessage
                            {
                                Role = "system",
                                Content = summary
                            });
                        }
                    }
                    break;
                }

                optimizedMessages.Insert(0, new ChatMessage
                {
                    Role = message.Role,
                    Content = message.Content
                });

                currentTokens += messageTokens;
            }

            var finalTokens = TokenCounter.EstimateRequestTokens(optimizedMessages);
            _logger.LogInformation("Optimized context for session {SessionId}: {MessageCount} messages, ~{TokenCount} tokens",
                sessionId, optimizedMessages.Count, finalTokens);

            return optimizedMessages;
        }

        public void ClearSessionCache(string sessionId)
        {
            var cacheKey = $"session_messages_{sessionId}";
            _memoryCache.Remove(cacheKey);
            _logger.LogDebug("Cleared cache for session {SessionId}", sessionId);
        }

        private async Task UpdateSessionInDatabase(ConversationSession session)
        {
            try
            {
                var sessionEntity = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionId == session.SessionId);

                if (sessionEntity != null)
                {
                    sessionEntity.MessagesJson = session.MessagesJson;
                    sessionEntity.MessageCount = session.MessageCount;
                    sessionEntity.LastActivityAt = session.LastActivityAt;
                    sessionEntity.Title = session.Title;

                    await _context.SaveChangesAsync();

                    _logger.LogDebug("Updated session {SessionId} in database with {MessageCount} messages",
                        session.SessionId, session.MessageCount);
                }
                else
                {
                    _logger.LogWarning("Session {SessionId} not found in database for update", session.SessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session {SessionId} in database", session.SessionId);
                throw;
            }
        }
    }
}