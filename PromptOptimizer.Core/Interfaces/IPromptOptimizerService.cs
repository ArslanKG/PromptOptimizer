using PromptOptimizer.Core.DTOs;

public interface IPromptOptimizerService
{
    Task<string> OptimizePromptAsync(
        string originalPrompt,
        string optimizationType = "clarity",
        string model = "gpt-4o-mini",
        List<ConversationMessage>? context = null,
        CancellationToken cancellationToken = default);
}