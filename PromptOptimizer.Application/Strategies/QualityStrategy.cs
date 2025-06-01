using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.Constants;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Strategies;

public class QualityStrategy : BaseStrategy
{
    public override string Name => "quality";

    public QualityStrategy(
        ICortexApiClient cortexClient,
        IPromptOptimizerService optimizerService,
        ILogger<QualityStrategy> logger)
        : base(cortexClient, optimizerService, logger)
    {
    }

    public override async Task<OptimizationResponse> ExecuteAsync(
        OptimizationRequest request,
        CancellationToken cancellationToken = default)
    {
        Stopwatch.Start();
        try
        {
            // Step 1: Optimize prompt
            var optimizationModel = "gpt-4o-mini";
            ModelsUsed.Add(optimizationModel);

            var optimizedPrompt = await OptimizerService.OptimizePromptAsync(
                request.Prompt,
                request.OptimizationType,
                optimizationModel,
                cancellationToken);

            Logger.LogInformation(LogMessages.OptimizedPrompt, optimizedPrompt);

            // Step 2: Get response with powerful model
            var responseModel = request.PreferredModels?.FirstOrDefault(m =>
                ModelInfo.EnabledModels.ContainsKey(m) &&
                ModelInfo.EnabledModels[m].Type == "advanced") ?? "gpt-4o";
            ModelsUsed.Add(responseModel);

            var responseRequest = new ChatCompletionRequest
            {
                Model = responseModel,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "user", Content = optimizedPrompt }
                },
                Temperature = 0.7
            };

            var response = await CortexClient.CreateChatCompletionAsync(
                responseRequest, cancellationToken);

            var content = response.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(content))
            {
                Logger.LogWarning(LogMessages.EmptyResponse, responseModel);
                throw new InvalidOperationException(
                    string.Format(ErrorMessages.NoResponseFromModel, responseModel));
            }

            return CreateResponse(
                request,
                optimizedPrompt,
                content,
                new Dictionary<string, object>
                {
                    ["optimization_type"] = request.OptimizationType,
                    ["total_tokens"] = response.Usage?.TotalTokens ?? 0
                });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, LogMessages.ErrorInStrategy, Name);
            throw;
        }
        finally
        {
            Stopwatch.Stop();
        }
    }
}