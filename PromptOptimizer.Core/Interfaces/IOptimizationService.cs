using PromptOptimizer.Core.Configuration;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;

namespace PromptOptimizer.Core.Interfaces;

public interface IOptimizationService
{
    Task<OptimizationResponse> OptimizeAsync(OptimizationRequest request, CancellationToken cancellationToken = default);
    Task<Dictionary<string, ModelInfo>> GetAvailableModelsAsync();
    Task<List<StrategyInfo>> GetStrategiesAsync();
    Task<List<OptimizationType>> GetOptimizationTypesAsync();
    Task<ModelTestResponse> TestModelAsync(string model, string prompt, CancellationToken cancellationToken = default);
}