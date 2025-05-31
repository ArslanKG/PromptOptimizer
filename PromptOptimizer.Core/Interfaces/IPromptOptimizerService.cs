namespace PromptOptimizer.Core.Interfaces;

public interface IPromptOptimizerService
{
    Task<string> OptimizePromptAsync(
        string originalPrompt,
        string optimizationType = "clarity",
        string model = "gpt-4o-mini",
        CancellationToken cancellationToken = default);
}