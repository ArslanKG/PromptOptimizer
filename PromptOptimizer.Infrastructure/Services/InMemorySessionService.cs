using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Infrastructure.Services
{
    public class InMemorySessionService : ISessionService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<InMemorySessionService> _logger;
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(24);

        public InMemorySessionService(
            IMemoryCache cache,
            ILogger<InMemorySessionService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<ConversationSession> CreateSessionAsync(string? userId = null)
        {
            var session = new ConversationSession
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _sessionTimeout,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            };

            _cache.Set($"session_{session.SessionId}", session, cacheOptions);
            _logger.LogInformation("Created new session: {SessionId}", session.SessionId);

            return Task.FromResult(session);
        }

        public Task<ConversationSession?> GetSessionAsync(string sessionId)
        {
            if (_cache.TryGetValue($"session_{sessionId}", out ConversationSession? session))
            {
                return Task.FromResult(session);
            }
            return Task.FromResult<ConversationSession?>(null);
        }

        public async Task<ConversationSession> AddMessageAsync(string sessionId, ConversationMessage message)
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null)
            {
                session = await CreateSessionAsync();
                sessionId = session.SessionId;
            }

            session.Messages.Add(message);
            session.LastActivityAt = DateTime.UtcNow;

            if (session.Messages.Count > session.MaxMessages)
            {
                var messagesToKeep = session.Messages
                    .OrderByDescending(m => m.Timestamp)
                    .Take(session.MaxMessages)
                    .OrderBy(m => m.Timestamp)
                    .ToList();
                session.Messages = messagesToKeep;
            }

            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _sessionTimeout,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            };
            _cache.Set($"session_{sessionId}", session, cacheOptions);

            _logger.LogInformation("Added message to session {SessionId}. Total messages: {Count}",
                sessionId, session.Messages.Count);

            return session;
        }

        public async Task<List<ConversationMessage>> GetConversationContextAsync(
            string sessionId, int? windowSize = null)
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null || !session.Messages.Any())
            {
                return new List<ConversationMessage>();
            }

            var size = windowSize ?? 10;
            return session.Messages
                .OrderByDescending(m => m.Timestamp)
                .Take(size)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        public Task<bool> ClearSessionAsync(string sessionId)
        {
            _cache.Remove($"session_{sessionId}");
            _logger.LogInformation("Cleared session: {SessionId}", sessionId);
            return Task.FromResult(true);
        }

        public Task<List<SessionResponse>> GetActiveSessionsAsync(string? userId = null)
        {
            return Task.FromResult(new List<SessionResponse>());
        }

        public async Task<ConversationHistoryResponse> GetConversationHistoryAsync(
            string sessionId, int? limit = null)
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null)
            {
                return new ConversationHistoryResponse { SessionId = sessionId };
            }

            var messages = limit.HasValue
                ? session.Messages.OrderByDescending(m => m.Timestamp).Take(limit.Value).OrderBy(m => m.Timestamp).ToList()
                : session.Messages;

            return new ConversationHistoryResponse
            {
                SessionId = sessionId,
                Messages = messages,
                TotalMessages = session.Messages.Count,
                LastActivityAt = session.LastActivityAt
            };
        }

        public Task<bool> SessionExistsAsync(string sessionId)
        {
            return Task.FromResult(_cache.TryGetValue($"session_{sessionId}", out _));
        }

        public Task AddMessageAsync(string sessionId, string role, string content)
        {
            var message = new ConversationMessage
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            return AddMessageAsync(sessionId, message);
        }
    }
}