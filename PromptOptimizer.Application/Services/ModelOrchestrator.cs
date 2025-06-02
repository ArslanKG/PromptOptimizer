using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services;

public class ModelOrchestrator : IModelOrchestrator
{
    private readonly ICortexApiClient _cortexClient;
    private readonly IPromptOptimizerService _optimizerService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<ModelOrchestrator> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ISessionCacheService _sessionCacheService;

    public ModelOrchestrator(
        ICortexApiClient cortexClient,
        IPromptOptimizerService optimizerService,
        ISessionService sessionService,
        IHttpContextAccessor? httpContextAccessor,
        ISessionCacheService sessionCacheService,
        ILogger<ModelOrchestrator> logger)
    {
        _cortexClient = cortexClient;
        _optimizerService = optimizerService;
        _sessionService = sessionService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _sessionCacheService = sessionCacheService;
    }

    public async Task<OptimizationResponse> ProcessPromptAsync(
        OptimizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var modelsUsed = new List<string>();

        try
        {
            var validStrategies = new[] { "quality", "speed", "consensus", "cost_effective" };
            if (!validStrategies.Contains(request.Strategy.ToLower()))
            {
                throw new ArgumentException($"Invalid strategy: '{request.Strategy}'");
            }

            ConversationSession? session = null;
            if (request.EnableMemory)
            {
                int? currentUserId = GetCurrentUserId();
                _logger.LogInformation("Current user ID from JWT: {UserId}", currentUserId);

                if (string.IsNullOrEmpty(request.SessionId))
                {
                    session = await _sessionService.CreateSessionAsync(currentUserId);
                    request.SessionId = session.SessionId;
                    _logger.LogInformation("Created new session {SessionId} for user {UserId}",
                        session.SessionId, currentUserId);
                }

                await _sessionCacheService.AddMessageToSessionAsync(request.SessionId, new ConversationMessage
                {
                    Role = "user",
                    Content = request.Prompt, // Original user prompt
                    Timestamp = DateTime.UtcNow
                });
            }

            var response = request.Strategy.ToLower() switch
            {
                "quality" => await QualityStrategyAsync(request, modelsUsed, cancellationToken),
                "speed" => await SpeedStrategyAsync(request, modelsUsed, cancellationToken),
                "consensus" => await ConsensusStrategyAsync(request, modelsUsed, cancellationToken),
                "cost_effective" => await CostEffectiveStrategyAsync(request, modelsUsed, cancellationToken),
                _ => throw new ArgumentException($"Strategy '{request.Strategy}' is not implemented")
            };

            if (request.EnableMemory && !string.IsNullOrEmpty(request.SessionId))
            {
                await _sessionCacheService.AddMessageToSessionAsync(request.SessionId, new ConversationMessage
                {
                    Role = "assistant",
                    Content = response.FinalResponse,
                    Model = modelsUsed.LastOrDefault(), 
                    Timestamp = DateTime.UtcNow
                });
            }

            response.Metadata ??= new Dictionary<string, object>();
            response.Metadata["sessionId"] = request.SessionId ?? "";

            return response;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessPromptAsync");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Completed processing with strategy {Strategy} in {ElapsedMs}ms using models: {Models}",
                request.Strategy,
                stopwatch.ElapsedMilliseconds,
                string.Join(", ", modelsUsed));
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

            var chatRequest = await BuildRequestWithContext(
                request,
                request.Prompt,
                cheapestModel,
                0.7,
                1000);

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

            _logger.LogInformation("Starting consensus strategy with models: {Models}",
                string.Join(", ", consensusModels));

            foreach (var model in consensusModels)
            {
                modelsUsed.Add(model);
                var chatRequest = await BuildRequestWithContext(
                    request,
                    request.Prompt,
                    model,
                    0.7,
                    500);

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

            string optimizedPrompt;

            if (request.EnableMemory && !string.IsNullOrEmpty(request.SessionId))
            {
                optimizedPrompt = await _optimizerService.OptimizePromptWithSessionAsync(
                    request.Prompt,
                    request.OptimizationType,
                    optimizationModel,
                    request.SessionId,
                    _sessionCacheService,
                    cancellationToken);
            }
            else
            {
                optimizedPrompt = await _optimizerService.OptimizePromptAsync(
                    request.Prompt,
                    request.OptimizationType,
                    optimizationModel,
                    null,
                    cancellationToken);
            }

            _logger.LogInformation("Original: '{Original}' → Optimized: '{Optimized}'",
                request.Prompt, optimizedPrompt);

            var responseModel = "gpt-4o";
            modelsUsed.Add(responseModel);

            var finalPrompt = $"{optimizedPrompt}\n\n(Lütfen cevabını Türkçe olarak ver.)";
            var chatRequest = await BuildRequestWithContext(request, finalPrompt, responseModel, 0.7);
            var response = await _cortexClient.CreateChatCompletionAsync(chatRequest, cancellationToken);

            return new OptimizationResponse
            {
                OriginalPrompt = request.Prompt,
                OptimizedPrompt = optimizedPrompt,
                FinalResponse = response.Choices?.FirstOrDefault()?.Message?.Content ?? "",
                ModelsUsed = modelsUsed,
                Strategy = request.Strategy,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Metadata = new Dictionary<string, object>
                {
                    ["optimization_type"] = request.OptimizationType,
                    ["total_tokens"] = response.Usage?.TotalTokens ?? 0
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in QualityStrategyAsync");
            throw;
        }
    }
    private async Task<ChatCompletionRequest> BuildRequestWithContext(
        OptimizationRequest request,
        string prompt,
        string model,
        double temperature = 0.7,
        int? maxTokens = null)
    {
        var messages = new List<ChatMessage>();

        // Optimized context al (eğer session varsa)
        if (request.EnableMemory && !string.IsNullOrEmpty(request.SessionId))
        {
            var contextMessages = await _sessionCacheService.GetOptimizedContextAsync(
                request.SessionId,
                4000); // 4000 token limit

            messages.AddRange(contextMessages);

            _logger.LogInformation("Added {Count} optimized context messages for session {SessionId}",
                contextMessages.Count, request.SessionId);
        }

        // Yeni mesajı ekle
        messages.Add(new ChatMessage
        {
            Role = "user",
            Content = prompt
        });

        var chatRequest = new ChatCompletionRequest
        {
            Model = model,
            Messages = messages,
            Temperature = temperature
        };

        if (maxTokens.HasValue)
        {
            chatRequest.MaxTokens = maxTokens.Value;
        }

        return chatRequest;
    }

    private ModelInfo SelectModelByStrategy(string strategy, string? optimizationType = null)
    {
        var availableModels = ModelInfo.EnabledModels
            .Select(kvp => kvp.Value)
            .ToList();

        return strategy.ToLower() switch
        {
            "quality" => availableModels
                .Where(m => m.Type == "advanced")
                .OrderByDescending(m => m.Cost)
                .FirstOrDefault() ?? availableModels.First(),

            "speed" => availableModels
                .Where(m => m.Type == "fast")
                .OrderBy(m => m.Cost)
                .FirstOrDefault() ?? availableModels.First(),

            "cost_effective" => availableModels
                .OrderBy(m => m.Cost)
                .First(),

            "consensus" => availableModels
                .Where(m => m.Type == "balanced")
                .FirstOrDefault() ?? availableModels.First(),

            _ => availableModels.First()
        };
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

        var chatRequest = await BuildRequestWithContext(
            request,
            request.Prompt,
            fastModel,
            0.5,
            500);

        if (chatRequest.Messages.Count == 1)
        {
            chatRequest.Messages.Insert(0, new ChatMessage
            {
                Role = "system",
                Content = "Kısa, net ve hızlı cevaplar ver."
            });
        }

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

    private int? GetCurrentUserId()
    {
        try
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogDebug("Successfully extracted user ID from JWT: {UserId}", userId);
                    return userId;
                }
                else
                {
                    _logger.LogWarning("User ID claim found but could not parse: {Claim}", userIdClaim);
                }
            }
            else
            {
                _logger.LogWarning("User is not authenticated or HttpContext is null");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user ID from JWT claims");
            return null;
        }
    }
}