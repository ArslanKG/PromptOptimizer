using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces;

public interface IOptimizationStrategy
{
    string Name { get; }
    Task<OptimizationResponse> ExecuteAsync(
        OptimizationRequest request,
        CancellationToken cancellationToken = default);
}