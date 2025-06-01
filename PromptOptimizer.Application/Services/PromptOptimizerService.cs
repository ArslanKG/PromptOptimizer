using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services;

public class PromptOptimizerService : IPromptOptimizerService
{
    private readonly ICortexApiClient _cortexClient;
    private readonly ILogger<PromptOptimizerService> _logger;

    private readonly Dictionary<string, string> _optimizationPrompts = new()
    {
        ["clarity"] = @"You are a prompt optimizer. Your task is to improve the clarity of the user's prompt.
Make it more specific and clear, but keep the original intent and context.
If the prompt refers to a previous conversation, maintain that reference.
Do not change the topic or add new information not implied in the original prompt.
Return only the optimized prompt without any explanation.",

        ["technical"] = @"You are a technical prompt optimizer. Enhance the prompt with technical precision.
Keep the original intent and any references to previous conversation.
Do not change the topic. Return only the optimized prompt.",

        ["creative"] = @"You are a creative prompt optimizer. Make the prompt more engaging and creative.
Maintain the original intent and context references.
Do not change the topic. Return only the optimized prompt.",

        ["analytical"] = @"You are an analytical prompt optimizer. Structure the prompt for analytical thinking.
Keep the original intent and conversation context.
Do not change the topic. Return only the optimized prompt."
    };

    public PromptOptimizerService(
        ICortexApiClient cortexClient,
        ILogger<PromptOptimizerService> logger)
    {
        _cortexClient = cortexClient;
        _logger = logger;
    }

    public async Task<string> OptimizePromptAsync(
        string originalPrompt,
        string optimizationType = "clarity",
        string model = "gpt-4o-mini",
        List<ConversationMessage>? context = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var systemPrompt = _optimizationPrompts.GetValueOrDefault(optimizationType)
                ?? _optimizationPrompts["clarity"];

            // Context varsa, optimization prompt'una ekle
            var optimizationPrompt = originalPrompt;
            if (context?.Any() == true)
            {
                // Son 3 mesajı al (çok fazla context karışıklık yaratabilir)
                var recentMessages = context.TakeLast(3).ToList();
                var contextText = string.Join("\n", recentMessages.Select(m =>
                    $"{(m.Role == "user" ? "User" : "Assistant")}: {(m.Content.Length > 100 ? m.Content[..100] + "..." : m.Content)}"));

                optimizationPrompt = $@"Given this recent conversation context:
{contextText}

The user now asks: '{originalPrompt}'

Optimize this prompt to be clear and contextually relevant. If it refers to something from the conversation above, make it explicit.
For example, if user asks 'What are the benefits?' and the context was about Python, optimize it to 'What are the benefits of Python?'";
            }
            else
            {
                optimizationPrompt = $"Optimize this prompt to be more clear and specific: '{originalPrompt}'";
            }

            var request = new ChatCompletionRequest
            {
                Model = model,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = optimizationPrompt }
                },
                Temperature = 0.3,
                MaxTokens = 150
            };

            var response = await _cortexClient.CreateChatCompletionAsync(request, cancellationToken);

            var optimizedPrompt = response.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrEmpty(optimizedPrompt))
            {
                _logger.LogWarning("No optimization received, using original prompt");
                return originalPrompt;
            }

            if (optimizedPrompt.Length > originalPrompt.Length * 3)
            {
                _logger.LogWarning("Optimization too long, using original prompt");
                return originalPrompt;
            }

            _logger.LogInformation("Prompt optimized from '{Original}' to '{Optimized}'",
                originalPrompt, optimizedPrompt);

            return optimizedPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to optimize prompt, using original");
            return originalPrompt; // Hata durumunda orijinal prompt'u kullan
        }
    }
}