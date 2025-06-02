using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services
{
    public class SessionManagementService : ISessionManagementService
    {
        private readonly ISessionService _sessionService;
        private readonly ISessionCacheService _sessionCacheService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SessionManagementService> _logger;

        public SessionManagementService(
            ISessionService sessionService,
            ISessionCacheService sessionCacheService,
            IMemoryCache cache,
            ILogger<SessionManagementService> logger)
        {
            _sessionService = sessionService;
            _sessionCacheService = sessionCacheService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<SessionOperationResult<SessionResponse>> GetSessionAsync(string sessionId, int userId)
        {
            try
            {
                if (!IsValidSessionId(sessionId))
                {
                    return SessionOperationResult<SessionResponse>.Failure("Invalid session ID format");
                }

                var session = await _sessionService.GetSessionAsync(sessionId);
                if (session == null)
                {
                    return SessionOperationResult<SessionResponse>.NotFound("Session not found");
                }

                if (session.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted unauthorized access to session {SessionId}", userId, sessionId);
                    return SessionOperationResult<SessionResponse>.Forbidden("Access denied");
                }

                var response = new SessionResponse
                {
                    SessionId = session.SessionId,
                    CreatedAt = session.CreatedAt,
                    MessageCount = session.Messages?.Count ?? session.MessageCount,
                    IsActive = session.IsActive,
                    LastActivityAt = session.LastActivityAt,
                    Title = session.Title ?? "Untitled Session",
                };

                return SessionOperationResult<SessionResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session {SessionId} for user {UserId}", sessionId, userId);
                return SessionOperationResult<SessionResponse>.Error("Failed to retrieve session");
            }
        }

        public async Task<SessionOperationResult<ConversationHistoryResponse>> GetHistoryAsync(
            string sessionId, int userId, int? limit = null)
        {
            try
            {
                if (!IsValidSessionId(sessionId))
                {
                    return SessionOperationResult<ConversationHistoryResponse>.Failure("Invalid session ID format");
                }

                if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
                {
                    return SessionOperationResult<ConversationHistoryResponse>.Failure("Limit must be between 1 and 100");
                }

                if (!await ValidateSessionAccessAsync(sessionId, userId))
                {
                    return SessionOperationResult<ConversationHistoryResponse>.Forbidden("Access denied");
                }

                var history = await GetHistoryWithCache(sessionId, limit);
                return SessionOperationResult<ConversationHistoryResponse>.Success(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history for session {SessionId}", sessionId);
                return SessionOperationResult<ConversationHistoryResponse>.Error("Failed to retrieve conversation history");
            }
        }

        public async Task<SessionOperationResult<bool>> ClearSessionAsync(string sessionId, int userId)
        {
            try
            {
                if (!IsValidSessionId(sessionId))
                {
                    return SessionOperationResult<bool>.Failure("Invalid session ID format");
                }

                var session = await _sessionService.GetSessionAsync(sessionId);
                if (session == null)
                {
                    return SessionOperationResult<bool>.NotFound("Session not found");
                }

                if (session.UserId != userId)
                {
                    return SessionOperationResult<bool>.Forbidden("Access denied");
                }

                _sessionCacheService.ClearSessionCache(sessionId);
                var result = await _sessionService.ClearSessionAsync(sessionId);

                if (result)
                {
                    _logger.LogInformation("Session {SessionId} cleared by user {UserId}", sessionId, userId);
                    return SessionOperationResult<bool>.Success(true);
                }

                return SessionOperationResult<bool>.NotFound("Session not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing session {SessionId} for user {UserId}", sessionId, userId);
                return SessionOperationResult<bool>.Error("Failed to clear session");
            }
        }

        public async Task<SessionOperationResult<List<SessionResponse>>> GetUserSessionsAsync(int userId, int? limit = null)
        {
            try
            {
                var sessions = await _sessionService.GetActiveSessionsAsync(userId);

                if (limit.HasValue && limit.Value > 0)
                {
                    sessions = sessions.Take(limit.Value).ToList();
                }

                return SessionOperationResult<List<SessionResponse>>.Success(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions for user {UserId}", userId);
                return SessionOperationResult<List<SessionResponse>>.Error("Failed to retrieve sessions");
            }
        }

        public async Task<bool> ValidateSessionAccessAsync(string sessionId, int userId)
        {
            var session = await _sessionService.GetSessionAsync(sessionId);
            return session != null && session.UserId == userId;
        }

        private async Task<ConversationHistoryResponse> GetHistoryWithCache(string sessionId, int? limit)
        {
            var cacheKey = $"history_{sessionId}_{limit ?? 0}";

            if (_cache.TryGetValue(cacheKey, out ConversationHistoryResponse? cachedHistory))
            {
                return cachedHistory!;
            }

            var history = await _sessionService.GetConversationHistoryAsync(sessionId, limit);
            _cache.Set(cacheKey, history, TimeSpan.FromMinutes(5));

            return history;
        }

        private static bool IsValidSessionId(string sessionId)
        {
            return !string.IsNullOrWhiteSpace(sessionId) &&
                   Guid.TryParse(sessionId, out _) &&
                   sessionId.Length == 36;
        }
    }
}