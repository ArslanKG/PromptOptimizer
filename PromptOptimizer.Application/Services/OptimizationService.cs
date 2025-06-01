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
    private readonly ICortexApiClient _cortexClient;
    private readonly ILogger<OptimizationService> _logger;
    private readonly StrategyConfiguration _configuration;

    public OptimizationService(
        IModelOrchestrator orchestrator,
        ICortexApiClient cortexClient,
        ILogger<OptimizationService> logger)
    {
        _orchestrator = orchestrator;
        _cortexClient = cortexClient;
        _logger = logger;
        _configuration = StrategyConfiguration.GetDefault();
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

    public Task<Dictionary<string, ModelInfo>> GetAvailableModelsAsync()
    {
        _logger.LogInformation(LogMessages.FetchingModels);
        return Task.FromResult(ModelInfo.EnabledModels);
    }

    public Task<List<StrategyInfo>> GetStrategiesAsync()
    {
        var strategies = _configuration.Strategies;
        _logger.LogInformation(LogMessages.ReturningStrategies, strategies.Count);
        return Task.FromResult(strategies);
    }

    public Task<List<OptimizationType>> GetOptimizationTypesAsync()
    {
        return Task.FromResult(_configuration.OptimizationTypes);
    }

    public async Task<ModelTestResponse> TestModelAsync(
        string model,
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var chatRequest = new ChatCompletionRequest
        {
            Model = model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.7
        };

        var response = await _cortexClient.CreateChatCompletionAsync(
            chatRequest, cancellationToken);

        return new ModelTestResponse
        {
            Model = model,
            Response = response.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty,
            Usage = response.Usage
        };
    }
}