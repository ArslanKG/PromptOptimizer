using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces;

public interface IOptimizationService
{
    Task<OptimizationResponse> OptimizeAsync(OptimizationRequest request, CancellationToken cancellationToken = default);
}