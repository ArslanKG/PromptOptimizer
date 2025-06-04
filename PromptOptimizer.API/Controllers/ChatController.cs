using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    public class ChatController : BaseApiController
    {
        private readonly IChatService _chatService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IValidationService _validationService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatService chatService,
            IRateLimitService rateLimitService,
            IValidationService validationService,
            ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _rateLimitService = rateLimitService;
            _validationService = validationService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(
            [FromBody] ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return UnauthorizedResult();

            var validation = _validationService.ValidateChatRequest(request);
            if (!validation.IsValid) return ValidationError(validation.ErrorMessage);

            if (!await _rateLimitService.CheckRateLimitAsync(userId.Value, "chat"))
                return RateLimitResult();

            try
            {
                var response = await _chatService.SendMessageAsync(
                    request.Message, request.Model, request.SessionId,
                    request.Temperature, request.MaxTokens, userId.Value,
                    cancellationToken);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return ValidationError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat failed for user {UserId}", userId.Value);
                return ServiceError();
            }
        }

        [HttpPost("strategy")]
        public async Task<IActionResult> SendWithStrategy(
            [FromBody] StrategyRequest request,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return UnauthorizedResult();

            var validation = _validationService.ValidateStrategyRequest(request);
            if (!validation.IsValid) return ValidationError(validation.ErrorMessage);

            if (!await _rateLimitService.CheckRateLimitAsync(userId.Value, "chat"))
                return RateLimitResult();

            try
            {
                var response = await _chatService.SendMessageWithStrategyAsync(
                    request.Message, request.Strategy, request.SessionId, userId, cancellationToken);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Strategy chat failed for user {UserId}", userId.Value);
                return ServiceError();
            }
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            try
            {
                var models = await _chatService.GetAvailableModelsAsync();
                return Ok(new ModelListResponse { Models = models });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get models");
                return ServiceError("Failed to retrieve models");
            }
        }
    }
}