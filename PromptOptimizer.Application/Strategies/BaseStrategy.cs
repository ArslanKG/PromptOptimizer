using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.Constants;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Strategies;

public abstract class BaseStrategy : IOptimizationStrategy
{
    protected readonly ICortexApiClient CortexClient;
    protected readonly IPromptOptimizerService OptimizerService;
    protected readonly ISessionService SessionService;
    protected readonly ILogger Logger;
    protected readonly List<string> ModelsUsed = new();
    protected readonly Stopwatch Stopwatch = new();

    public abstract string Name { get; }

    protected BaseStrategy(
        ICortexApiClient cortexClient,
        IPromptOptimizerService optimizerService,
        ISessionService sessionService,
        ILogger logger)
    {
        CortexClient = cortexClient;
        OptimizerService = optimizerService;
        SessionService = sessionService;
        Logger = logger;
    }

    public abstract Task<OptimizationResponse> ExecuteAsync(
        OptimizationRequest request,
        CancellationToken cancellationToken = default);

    protected OptimizationResponse CreateResponse(
        OptimizationRequest request,
        string optimizedPrompt,
        string finalResponse,
        Dictionary<string, object>? metadata = null)
    {
        return new OptimizationResponse
        {
            OriginalPrompt = request.Prompt,
            OptimizedPrompt = optimizedPrompt,
            FinalResponse = finalResponse,
            ModelsUsed = ModelsUsed.Distinct().ToList(),
            Strategy = request.Strategy,
            ProcessingTimeMs = Stopwatch.ElapsedMilliseconds,
            Metadata = metadata
        };
    }
}