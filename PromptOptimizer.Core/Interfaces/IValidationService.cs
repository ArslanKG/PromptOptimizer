using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces
{
    public interface IValidationService
    {
        ValidationResult ValidateChatRequest(ChatRequest request);
        ValidationResult ValidateStrategyRequest(StrategyRequest request);
        ValidationResult ValidateModel(string modelName);
        ValidationResult ValidateTemperature(double temperature);
        ValidationResult ValidateSessionId(string sessionId);
        ValidationResult ValidateLimit(int? limit, int maxLimit = 100);
    }
}