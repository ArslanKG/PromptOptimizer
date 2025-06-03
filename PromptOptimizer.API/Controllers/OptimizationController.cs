using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;
using PromptOptimizer.Core.Models;

namespace PromptOptimizer.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OptimizationController : ControllerBase
    {
        private readonly IOptimizationService _optimizationService;
        private readonly ISessionManagementService _sessionManagementService;
        private readonly IValidationService _validationService;
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<OptimizationController> _logger;

        public OptimizationController(
            IOptimizationService optimizationService,
            ISessionManagementService sessionManagementService,
            IValidationService validationService,
            IRateLimitService rateLimitService,
            ILogger<OptimizationController> logger)
        {
            _optimizationService = optimizationService;
            _sessionManagementService = sessionManagementService;
            _validationService = validationService;
            _rateLimitService = rateLimitService;
            _logger = logger;
        }

        [HttpPost("optimize")]
        public async Task<IActionResult> OptimizePrompt([FromBody] OptimizationRequest request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized(new ErrorResponse("Authentication required"));

            if (!await _rateLimitService.CheckRateLimitAsync(userId.Value, "optimize"))
                return StatusCode(429, new ErrorResponse("Rate limit exceeded"));

            var validation = _validationService.ValidateOptimizationRequest(request);
            if (!validation.IsValid) return BadRequest(new ErrorResponse(validation.ErrorMessage));

            try
            {
                var response = await _optimizationService.OptimizeAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new ErrorResponse("Request timeout"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Optimization failed for user {UserId}", userId);
                return StatusCode(500, new ErrorResponse("Service temporarily unavailable"));
            }
        }

        [AllowAnonymous]
        [HttpGet("health")]
        [ResponseCache(Duration = 30)]
        public IActionResult HealthCheck() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSession(string sessionId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var result = await _sessionManagementService.GetSessionAsync(sessionId, userId.Value);
            return ConvertToActionResult(result);
        }

        [HttpGet("sessions/{sessionId}/history")]
        public async Task<IActionResult> GetConversationHistory(string sessionId, [FromQuery] int? limit = null)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var result = await _sessionManagementService.GetHistoryAsync(sessionId, userId.Value, limit);
            return ConvertToActionResult(result);
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> ClearSession(string sessionId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var result = await _sessionManagementService.ClearSessionAsync(sessionId, userId.Value);
            return result.IsSuccess ? NoContent() : ConvertToActionResult(result);
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetUserSessions([FromQuery] int? limit = null)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var result = await _sessionManagementService.GetUserSessionsAsync(userId.Value, limit);
            return ConvertToActionResult(result);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private IActionResult ConvertToActionResult<T>(SessionOperationResult<T> result)
        {
            return result.Status switch
            {
                OperationStatus.Success => Ok(result.Data),
                OperationStatus.BadRequest => BadRequest(new ErrorResponse(result.ErrorMessage)),
                OperationStatus.NotFound => NotFound(new ErrorResponse(result.ErrorMessage)),
                OperationStatus.Forbidden => Forbid(),
                OperationStatus.InternalError => StatusCode(500, new ErrorResponse(result.ErrorMessage)),
                _ => StatusCode(500, new ErrorResponse("Unknown error"))
            };
        }
    }
}