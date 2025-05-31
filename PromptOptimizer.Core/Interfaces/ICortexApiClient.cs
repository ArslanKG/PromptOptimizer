using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces;

public interface ICortexApiClient
{
    Task<ChatCompletionResponse> CreateChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}