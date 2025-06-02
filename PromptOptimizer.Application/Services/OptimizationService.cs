using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.Configuration;
using PromptOptimizer.Core.Constants;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services;

public class OptimizationService : IOptimizationService
{
    private readonly IModelOrchestrator _orchestrator;
    private readonly ILogger<OptimizationService> _logger;

    public OptimizationService(
        IModelOrchestrator orchestrator,
        ILogger<OptimizationService> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<OptimizationResponse> OptimizeAsync(
        OptimizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request?.Prompt))
        {
            throw new ArgumentException(ErrorMessages.PromptCannotBeEmpty);
        }

        _logger.LogInformation(LogMessages.ProcessingOptimization, request.Strategy);

        return await _orchestrator.ProcessPromptAsync(request, cancellationToken);
    }
}