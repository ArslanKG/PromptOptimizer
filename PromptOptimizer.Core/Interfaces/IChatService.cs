// PromptOptimizer.Core/Interfaces/IChatService.cs
using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponse> SendMessageAsync(
            string message,
            string model,
            string? sessionId = null,
            double temperature = 0.7,
            int? maxTokens = null,
            int? userId = null, 
            CancellationToken cancellationToken = default);

        Task<ChatResponse> SendMessageWithStrategyAsync(
            string message,
            string strategy = "default",
            string? sessionId = null,
            int? userId = null,
            CancellationToken cancellationToken = default);

        Task<List<AvailableModel>> GetAvailableModelsAsync();

        Task<bool> ValidateModelAsync(string modelName);
    }
}