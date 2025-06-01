using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services;

public class ModelOrchestrator : IModelOrchestrator
{
    private readonly ICortexApiClient _cortexClient;
    private readonly IPromptOptimizerService _optimizerService;
    private readonly ILogger<ModelOrchestrator> _logger;

    public ModelOrchestrator(
        ICortexApiClient cortexClient,
        IPromptOptimizerService optimizerService,
        ILogger<ModelOrchestrator> logger)
    {
        _cortexClient = cortexClient;
        _optimizerService = optimizerService;
        _logger = logger;
    }

    public async Task<OptimizationResponse> ProcessPromptAsync(
        OptimizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var modelsUsed = new List<string>();

        try
        {
            return request.Strategy.ToLower() switch
            {
                "quality" => await QualityStrategyAsync(request, modelsUsed, cancellationToken),
                "speed" => await SpeedStrategyAsync(request, modelsUsed, cancellationToken),
                "consensus" => await ConsensusStrategyAsync(request, modelsUsed, cancellationToken),
                "cost_effective" => await CostEffectiveStrategyAsync(request, modelsUsed, cancellationToken),
                _ => await QualityStrategyAsync(request, modelsUsed, cancellationToken)
            };
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Processed prompt with strategy {Strategy} in {ElapsedMs}ms using models: {Models}",
                request.Strategy,
                stopwatch.ElapsedMilliseconds,
                string.Join(", ", modelsUsed));
        }
    }

    private async Task<OptimizationResponse> QualityStrategyAsync(
        OptimizationRequest request,
        List<string> modelsUsed,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var optimizationModel = "gpt-4o-mini";
            modelsUsed.Add(optimizationModel);

            var optimizedPrompt = await _optimizerService.OptimizePromptAsync(
                request.Prompt,
                request.OptimizationType,
                optimizationModel,
                cancellationToken);

            _logger.LogInformation("Optimized prompt: {OptimizedPrompt}", optimizedPrompt);

            var responseModel = request.PreferredModels?.FirstOrDefault(m =>
                ModelInfo.EnabledModels.ContainsKey(m) &&
                ModelInfo.EnabledModels[m].Type == "advanced") ?? "gpt-4o";
            modelsUsed.Add(responseModel);

            var responseRequest = new ChatCompletionRequest
            {
                Model = responseModel,
                Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = optimizedPrompt }
            },
                Temperature = 0.7
            };

            var initialResponse = await _cortexClient.CreateChatCompletionAsync(
                responseRequest,
                cancellationToken);

            var initialContent = initialResponse.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(initialContent))
            {
                _logger.LogWarning("Initial response is empty from model {Model}", responseModel);
                throw new InvalidOperationException($"No response from {responseModel}");
            }

            return new OptimizationResponse
            {
                OriginalPrompt = request.Prompt,
                OptimizedPrompt = optimizedPrompt,
                FinalResponse = initialContent,
                ModelsUsed = modelsUsed,
                Strategy = request.Strategy,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Metadata = new Dictionary<string, object>
                {
                    ["optimization_type"] = request.OptimizationType,
                    ["total_tokens"] = initialResponse.Usage?.TotalTokens ?? 0
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in QualityStrategyAsync");
            throw;
        }
    }

    private async Task<OptimizationResponse> SpeedStrategyAsync(
    OptimizationRequest request,
    List<string> modelsUsed,
    CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var fastModel = request.PreferredModels?.FirstOrDefault(m =>
            ModelInfo.AvailableModels[m].Type == "fast") ?? "gpt-4o-mini";
        modelsUsed.Add(fastModel);

        var chatRequest = new ChatCompletionRequest
        {
            Model = fastModel,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "Kısa, net ve hızlı cevaplar ver." },
                new() { Role = "user", Content = request.Prompt }
            },
            Temperature = 0.5,
            MaxTokens = 500
        };

        var response = await _cortexClient.CreateChatCompletionAsync(
            chatRequest,
            cancellationToken);

        return new OptimizationResponse
        {
            OriginalPrompt = request.Prompt,
            OptimizedPrompt = request.Prompt,
            FinalResponse = response.Choices[0].Message.Content,
            ModelsUsed = modelsUsed,
            Strategy = request.Strategy,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    private async Task<OptimizationResponse> ConsensusStrategyAsync(
        OptimizationRequest request,
        List<string> modelsUsed,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var consensusModels = new[] { "gpt-4o-mini", "o3-mini", "grok-3-mini-beta" };
            var tasks = new List<Task<ChatCompletionResponse>>();

            _logger.LogInformation("Starting consensus strategy with models: {Models}", string.Join(", ", consensusModels));

            foreach (var model in consensusModels)
            {
                modelsUsed.Add(model);
                var chatRequest = new ChatCompletionRequest
                {
                    Model = model,
                    Messages = new List<ChatMessage>
                {
                    new() { Role = "user", Content = request.Prompt }
                },
                    Temperature = 0.7,
                    MaxTokens = 500
                };

                tasks.Add(_cortexClient.CreateChatCompletionAsync(chatRequest, cancellationToken));
            }

            var responses = await Task.WhenAll(tasks);

            _logger.LogInformation("All consensus models responded successfully");

            var synthesisModel = "gpt-4o-mini"; 
            modelsUsed.Add(synthesisModel);

            var validResponses = responses
                .Where(r => r.Choices?.FirstOrDefault()?.Message?.Content != null)
                .Select((r, i) => $"Model {i + 1} Cevabı:\n{r.Choices[0].Message.Content}")
                .ToList();

            if (!validResponses.Any())
            {
                throw new InvalidOperationException("No valid responses from consensus models");
            }

            var synthesisPrompt = $@"
            Aşağıdaki {validResponses.Count} farklı AI modelinden gelen cevapları analiz et ve en iyi kısımları birleştirerek tek bir kapsamlı cevap oluştur:
            
            {string.Join("\n\n", validResponses)}
            
            Birleştirilmiş cevap net, tutarlı ve tüm modellerin güçlü yanlarını içermelidir.";

            var synthesisRequest = new ChatCompletionRequest
            {
                Model = synthesisModel,
                Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = synthesisPrompt }
            },
                Temperature = 0.3,
                MaxTokens = 1000
            };

            _logger.LogInformation("Synthesizing responses with {Model}", synthesisModel);

            var finalResponse = await _cortexClient.CreateChatCompletionAsync(
                synthesisRequest,
                cancellationToken);

            var finalContent = finalResponse.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(finalContent))
            {
                throw new InvalidOperationException("Synthesis failed - no response content");
            }

            return new OptimizationResponse
            {
                OriginalPrompt = request.Prompt,
                OptimizedPrompt = request.Prompt,
                FinalResponse = finalContent,
                ModelsUsed = modelsUsed.Distinct().ToList(),
                Strategy = request.Strategy,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Metadata = new Dictionary<string, object>
                {
                    ["consensus_count"] = consensusModels.Length,
                    ["synthesis_model"] = synthesisModel,
                    ["valid_responses"] = validResponses.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ConsensusStrategyAsync");
            throw;
        }
    }

    private async Task<OptimizationResponse> CostEffectiveStrategyAsync(
        OptimizationRequest request,
        List<string> modelsUsed,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var cheapestModel = ModelInfo.EnabledModels
                .OrderBy(m => m.Value.Cost)
                .First().Key;

            modelsUsed.Add(cheapestModel);

            _logger.LogInformation("Using cheapest model: {Model} with cost: {Cost}",
                cheapestModel, ModelInfo.EnabledModels[cheapestModel].Cost);

            var chatRequest = new ChatCompletionRequest
            {
                Model = cheapestModel,
                Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = request.Prompt }
            },
                Temperature = 0.7,
                MaxTokens = 1000
            };

            var response = await _cortexClient.CreateChatCompletionAsync(
                chatRequest,
                cancellationToken);

            var content = response.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(content))
            {
                _logger.LogError("No response from model {Model}", cheapestModel);
                throw new InvalidOperationException($"No response from {cheapestModel}");
            }

            return new OptimizationResponse
            {
                OriginalPrompt = request.Prompt,
                OptimizedPrompt = request.Prompt,
                FinalResponse = content,
                ModelsUsed = modelsUsed,
                Strategy = request.Strategy,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Metadata = new Dictionary<string, object>
                {
                    ["estimated_cost"] = ModelInfo.EnabledModels[cheapestModel].Cost,
                    ["model_used"] = cheapestModel
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CostEffectiveStrategyAsync");
            throw;
        }
    }
}