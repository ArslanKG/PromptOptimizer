using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptimizationController : ControllerBase
    {
        private readonly IModelOrchestrator _orchestrator;
        private readonly ILogger<OptimizationController> _logger;

        public OptimizationController(
            IModelOrchestrator orchestrator,
            ILogger<OptimizationController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        /// <summary>
        /// Optimize and process a prompt using selected strategy
        /// </summary>
        /// <param name="request">Optimization request with prompt and settings</param>
        /// <returns>Optimized prompt and AI response</returns>
        [HttpPost("optimize")]
        [ProducesResponseType(typeof(OptimizationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OptimizePrompt(
            [FromBody] OptimizationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Prompt))
                {
                    return BadRequest(new { error = "Prompt cannot be empty" });
                }

                _logger.LogInformation(
                    "Processing optimization request with strategy: {Strategy}",
                    request.Strategy);

                var response = await _orchestrator.ProcessPromptAsync(request, cancellationToken);

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status408RequestTimeout,
                    new { error = "Request timeout" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing optimization request");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while processing your request", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all available AI models with their properties
        /// </summary>
        /// <returns>Dictionary of available models</returns>
        [HttpGet("models")]
        [ProducesResponseType(typeof(Dictionary<string, ModelInfo>), StatusCodes.Status200OK)]
        public IActionResult GetAvailableModels()
        {
            _logger.LogInformation("Fetching available models");
            return Ok(ModelInfo.EnabledModels);
        }

        /// <summary>
        /// Get all available optimization strategies
        /// </summary>
        /// <returns>List of strategies with descriptions</returns>
        [HttpGet("strategies")]
        [ProducesResponseType(typeof(List<StrategyInfo>), StatusCodes.Status200OK)]
        public IActionResult GetStrategies()
        {
            var strategies = new List<StrategyInfo>
            {
                new() {
                    Id = "quality",
                    Name = "En İyi Kalite",
                    Description = "Çoklu model kullanarak en kaliteli cevap",
                    EstimatedTime = "15-20 saniye"
                },
                new() {
                    Id = "speed",
                    Name = "Hızlı",
                    Description = "Hızlı modeller ile quick response",
                    EstimatedTime = "3-5 saniye"
                },
                new() {
                    Id = "consensus",
                    Name = "Konsensüs",
                    Description = "Birden fazla model görüşü",
                    EstimatedTime = "20-30 saniye"
                },
                new() {
                    Id = "cost_effective",
                    Name = "Maliyet Odaklı",
                    Description = "Uygun maliyetli çözüm",
                    EstimatedTime = "5-10 saniye"
                }
            };

            _logger.LogInformation("Returning {Count} strategies", strategies.Count);
            return Ok(strategies);
        }

        /// <summary>
        /// Get optimization types (clarity, technical, creative, analytical)
        /// </summary>
        /// <returns>List of optimization types</returns>
        [HttpGet("optimization-types")]
        [ProducesResponseType(typeof(List<OptimizationType>), StatusCodes.Status200OK)]
        public IActionResult GetOptimizationTypes()
        {
            var types = new List<OptimizationType>
            {
                new() {
                    Id = "clarity",
                    Name = "Netlik",
                    Description = "Belirsizlikleri giderir ve daha net hale getirir"
                },
                new() {
                    Id = "technical",
                    Name = "Teknik",
                    Description = "Teknik ve yazılım odaklı optimize eder"
                },
                new() {
                    Id = "creative",
                    Name = "Yaratıcı",
                    Description = "Yaratıcı içerik için optimize eder"
                },
                new() {
                    Id = "analytical",
                    Name = "Analitik",
                    Description = "Analitik ve veri odaklı optimize eder"
                }
            };

            return Ok(types);
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "PromptOptimizer.API",
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Test a specific model directly
        /// </summary>
        [HttpPost("test-model")]
        public async Task<IActionResult> TestModel(
            [FromBody] TestModelRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var chatRequest = new ChatCompletionRequest
                {
                    Model = request.Model,
                    Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = request.Prompt }
            },
                    Temperature = 0.7
                };

                var cortexClient = HttpContext.RequestServices.GetRequiredService<ICortexApiClient>();
                var response = await cortexClient.CreateChatCompletionAsync(chatRequest, cancellationToken);

                return Ok(new
                {
                    model = request.Model,
                    response = response.Choices?.FirstOrDefault()?.Message?.Content,
                    usage = response.Usage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
            }
        }

    }

    // DTOs for additional endpoints
    public class StrategyInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
    }

    public class OptimizationType
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TestModelRequest
    {
        public string Model { get; set; } = "gpt-4o-mini";
        public string Prompt { get; set; } = "";
    }
}