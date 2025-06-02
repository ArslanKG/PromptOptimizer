using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

public interface IPromptOptimizerService
{
    Task<string> OptimizePromptAsync(
        string prompt,
        string optimizationType,
        string model,
        List<ConversationMessage>? context = null,
        CancellationToken cancellationToken = default);

    Task<string> OptimizePromptWithSessionAsync(
        string prompt,
        string optimizationType,
        string model,
        string sessionId,
        ISessionCacheService sessionCacheService,
        CancellationToken cancellationToken = default);
}