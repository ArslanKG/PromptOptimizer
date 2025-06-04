
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
            Unauthorized(new ErrorResponse("Authentication required"));

        protected IActionResult RateLimitResult() =>
            StatusCode(429, new ErrorResponse("Rate limit exceeded"));

        protected IActionResult ValidationError(string message) =>
            BadRequest(new ErrorResponse(message));

        protected IActionResult ServiceError(string message = "Service temporarily unavailable") =>
            StatusCode(500, new ErrorResponse(message));
    }
}