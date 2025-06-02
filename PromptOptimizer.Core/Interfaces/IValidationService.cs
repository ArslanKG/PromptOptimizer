using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces
{
    public interface IValidationService
    {
        Core.DTOs.ValidationResult ValidateOptimizationRequest(OptimizationRequest request);
        Core.DTOs.ValidationResult ValidateSessionId(string sessionId);
        Core.DTOs.ValidationResult ValidateLimit(int? limit, int maxLimit = 100);
    }
}