using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.API.Controllers
{
    [Route("api/public/chat")]
    [ApiController]
    [AllowAnonymous] // Auth olmadan erişim
    public class PublicChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IValidationService _validationService;
        private readonly ILogger<PublicChatController> _logger;

        public PublicChatController(
            IChatService chatService,
            IRateLimitService rateLimitService,
            IValidationService validationService,
            ILogger<PublicChatController> logger)
        {
            _chatService = chatService;
            _rateLimitService = rateLimitService;
            _validationService = validationService;
            _logger = logger;
        }

        /// <summary>
        /// Public chat endpoint - Auth olmadan kullanım
        /// Saatlik 30 request limiti, sadece GPT-4o-mini, memory tutmaz
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendPublicMessage(
            [FromBody] PublicChatRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Client IP adresini al
                var clientIp = GetClientIpAddress();
                
                _logger.LogInformation("Public chat request from IP: {ClientIp}", clientIp);

                // Basic validation
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Message cannot be empty"));
                }

                if (request.Message.Length > 4000)
                {
                    return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Message too long. Maximum 4000 characters allowed."));
                }

                // IP bazlı rate limiting kontrolü
                if (!await _rateLimitService.CheckPublicRateLimitAsync(clientIp))
                {
                    var rateLimitInfo = await _rateLimitService.GetPublicRateLimitInfoAsync(clientIp);
                    
                    return StatusCode(429, new ErrorResponse(
                        "RATE_LIMIT_EXCEEDED", 
                        $"Public API rate limit exceeded. {rateLimitInfo.RemainingRequests} requests remaining. Resets at {rateLimitInfo.ResetTime:HH:mm} UTC.",
                        $"Limit: {rateLimitInfo.Limit}/hour, Used: {rateLimitInfo.RequestCount}"
                    ));
                }

                // Chat işlemi
                var response = await _chatService.SendPublicMessageAsync(request.Message, cancellationToken);

                // Rate limit bilgilerini response'a ekle
                var currentRateLimitInfo = await _rateLimitService.GetPublicRateLimitInfoAsync(clientIp);
                response.RemainingRequests = currentRateLimitInfo.RemainingRequests;
                response.ResetTime = currentRateLimitInfo.ResetTime;

                _logger.LogInformation("Public chat completed for IP {ClientIp}. Remaining: {Remaining}/{Limit}", 
                    clientIp, currentRateLimitInfo.RemainingRequests, currentRateLimitInfo.Limit);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Public chat validation error");
                return BadRequest(new ErrorResponse("VALIDATION_ERROR", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Public chat failed");
                return StatusCode(500, new ErrorResponse("SERVICE_ERROR", "Service temporarily unavailable"));
            }
        }

        /// <summary>
        /// Public API rate limit bilgilerini getir
        /// </summary>
        [HttpGet("rate-limit")]
        public async Task<IActionResult> GetRateLimit()
        {
            try
            {
                var clientIp = GetClientIpAddress();
                var rateLimitInfo = await _rateLimitService.GetPublicRateLimitInfoAsync(clientIp);

                return Ok(new
                {
                    requestCount = rateLimitInfo.RequestCount,
                    limit = rateLimitInfo.Limit,
                    remainingRequests = rateLimitInfo.RemainingRequests,
                    resetTime = rateLimitInfo.ResetTime,
                    isLimitExceeded = rateLimitInfo.IsLimitExceeded
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get public rate limit info");
                return StatusCode(500, new ErrorResponse("SERVICE_ERROR", "Failed to retrieve rate limit information"));
            }
        }

        /// <summary>
        /// Public API hakkında bilgi
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetPublicApiInfo()
        {
            return Ok(new
            {
                description = "Public Chat API - No authentication required",
                model = "gpt-4o-mini",
                rateLimit = new
                {
                    requests = 30,
                    period = "1 hour",
                    resetInterval = "Every hour at minute 0"
                },
                features = new
                {
                    authentication = false,
                    sessionMemory = false,
                    costOptimized = true,
                    maxTokens = 1000,
                    temperature = 0.7
                },
                limits = new
                {
                    maxMessageLength = 4000,
                    maxTokensPerResponse = 1000
                }
            });
        }

        private string GetClientIpAddress()
        {
            // X-Forwarded-For header'ını kontrol et (proxy/load balancer için)
            var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                // İlk IP adresini al (client IP)
                return xForwardedFor.Split(',')[0].Trim();
            }

            // X-Real-IP header'ını kontrol et
            var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            // RemoteIpAddress'i kullan
            return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}