using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Core.Configuration;
using PromptOptimizer.Core.Constants;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OptimizationController : ControllerBase
{
    private readonly IOptimizationService _optimizationService;
    private readonly ILogger<OptimizationController> _logger;

    public OptimizationController(
        IOptimizationService optimizationService,
        ILogger<OptimizationController> logger)
    {
        _optimizationService = optimizationService;
        _logger = logger;
    }

    /// <summary>
    /// Optimize and process a prompt using selected strategy
    /// </summary>
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
            var response = await _optimizationService.OptimizeAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status408RequestTimeout,
                new { error = ErrorMessages.RequestTimeout });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing optimization request");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = ErrorMessages.ProcessingError, details = ex.Message });
        }
    }

    /// <summary>
    /// Get all available AI models
    /// </summary>
    [HttpGet("models")]
    [ProducesResponseType(typeof(Dictionary<string, ModelInfo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableModels()
    {
        var models = await _optimizationService.GetAvailableModelsAsync();
        return Ok(models);
    }

    /// <summary>
    /// Get all available optimization strategies
    /// </summary>
    [HttpGet("strategies")]
    [ProducesResponseType(typeof(List<StrategyInfo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStrategies()
    {
        var strategies = await _optimizationService.GetStrategiesAsync();
        return Ok(strategies);
    }

    /// <summary>
    /// Get optimization types
    /// </summary>
    [HttpGet("optimization-types")]
    [ProducesResponseType(typeof(List<OptimizationType>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptimizationTypes()
    {
        var types = await _optimizationService.GetOptimizationTypesAsync();
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
    /// Test a specific model
    /// </summary>
    [HttpPost("test-model")]
    [ProducesResponseType(typeof(ModelTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestModel(
        [FromBody] TestModelRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _optimizationService.TestModelAsync(
                request.Model, request.Prompt, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class TestModelRequest
{
    public string Model { get; set; } = "gpt-4o-mini";
    public string Prompt { get; set; } = string.Empty;
}