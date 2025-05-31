using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces;

public interface IModelOrchestrator
{
    Task<OptimizationResponse> ProcessPromptAsync(
        OptimizationRequest request,
        CancellationToken cancellationToken = default);
}