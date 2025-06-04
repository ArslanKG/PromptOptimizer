using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    public class SessionsController : BaseApiController
    {
        private readonly ISessionService _sessionService;
        private readonly IValidationService _validationService;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(
            ISessionService sessionService,
            IValidationService validationService,
            ILogger<SessionsController> logger)
        {
            _sessionService = sessionService;
            _validationService = validationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSessions([FromQuery] int? limit = 20)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return UnauthorizedResult();

            var limitValidation = _validationService.ValidateLimit(limit);
            if (!limitValidation.IsValid) return ValidationError(limitValidation.ErrorMessage);

            try
            {
                var sessions = await _sessionService.GetActiveSessionsAsync(userId.Value);
                var result = limit.HasValue ? sessions.Take(limit.Value) : sessions;

                return Ok(new { sessions = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get sessions for user {UserId}", userId.Value);
                return ServiceError("Failed to retrieve sessions");
            }
        }

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetSession(string sessionId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return UnauthorizedResult();

            var validation = _validationService.ValidateSessionId(sessionId);
            if (!validation.IsValid) return ValidationError(validation.ErrorMessage);

            try
            {
                var session = await _sessionService.GetSessionAsync(sessionId);
                if (session == null || session.UserId != userId.Value)
                    return NotFound(new ErrorResponse("Session not found"));

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get session {SessionId}", sessionId);
                return ServiceError("Failed to retrieve session");
            }
        }

        [HttpGet("{sessionId}/history")]
        public async Task<IActionResult> GetHistory(string sessionId, [FromQuery] int? limit = null)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return UnauthorizedResult();

            var sessionValidation = _validationService.ValidateSessionId(sessionId);
            var limitValidation = _validationService.ValidateLimit(limit);

            var combinedValidation = sessionValidation.Combine(limitValidation);
            if (!combinedValidation.IsValid) return ValidationError(combinedValidation.ErrorMessage);

            try
            {
                var session = await _sessionService.GetSessionAsync(sessionId);
                if (session?.UserId != userId.Value)
                    return NotFound(new ErrorResponse("Session not found"));

                var history = await _sessionService.GetConversationHistoryAsync(sessionId, limit);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get history for session {SessionId}", sessionId);
                return ServiceError("Failed to retrieve history");
            }
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return UnauthorizedResult();

            var validation = _validationService.ValidateSessionId(sessionId);
            if (!validation.IsValid) return ValidationError(validation.ErrorMessage);

            try
            {
                var session = await _sessionService.GetSessionAsync(sessionId);
                if (session?.UserId != userId.Value)
                    return NotFound(new ErrorResponse("Session not found"));

                var success = await _sessionService.ClearSessionAsync(sessionId);
                if (!success) return ServiceError("Failed to delete session");

                _logger.LogInformation("Session {SessionId} deleted by user {UserId}", sessionId, userId.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete session {SessionId}", sessionId);
                return ServiceError("Failed to delete session");
            }
        }
    }
}