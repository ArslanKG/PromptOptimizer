
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.API.Controllers
{
    [Authorize]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        protected IActionResult UnauthorizedResult() =>
            Unauthorized(new ErrorResponse("AUTH_REQUIRED", "Authentication required"));

        protected IActionResult RateLimitResult() =>
            StatusCode(429, new ErrorResponse("RATE_LIMIT_EXCEEDED", "Rate limit exceeded. Please try again later."));

        protected IActionResult ValidationError(string message, string? errorCode = null) =>
            BadRequest(new ErrorResponse(errorCode ?? "VALIDATION_ERROR", message));

        protected IActionResult ServiceError(string? errorCode = null, string? details = null, bool logDetails = true)
        {
            var response = new ErrorResponse(
                errorCode ?? "SERVICE_ERROR",
                "Service temporarily unavailable",
                logDetails ? details : null
            );
            return StatusCode(500, response);
        }

        protected IActionResult NotFoundError(string resource, string identifier) =>
            NotFound(new ErrorResponse("RESOURCE_NOT_FOUND", $"{resource} with identifier '{identifier}' was not found"));
    }
}