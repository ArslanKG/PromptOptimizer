using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services
{
    public class ValidationService : IValidationService
    {
        public Core.DTOs.ValidationResult ValidateOptimizationRequest(OptimizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return Core.DTOs.ValidationResult.Invalid("Prompt cannot be empty");

            if (request.Prompt.Length > 5000)
                return Core.DTOs.ValidationResult.Invalid("Prompt too long (max 5000 characters)");

            var validStrategies = new[] { "quality", "speed", "consensus", "cost_effective" };
            if (!validStrategies.Contains(request.Strategy?.ToLower()))
                return Core.DTOs.ValidationResult.Invalid($"Invalid strategy. Valid options: {string.Join(", ", validStrategies)}");

            return Core.DTOs.ValidationResult.Valid();
        }

        public Core.DTOs.ValidationResult ValidateSessionId(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return Core.DTOs.ValidationResult.Invalid("Session ID cannot be empty");

            if (!Guid.TryParse(sessionId, out _) || sessionId.Length != 36)
                return Core.DTOs.ValidationResult.Invalid("Invalid session ID format");

            return Core.DTOs.ValidationResult.Valid();
        }

        public Core.DTOs.ValidationResult ValidateLimit(int? limit, int maxLimit = 100)
        {
            if (!limit.HasValue)
                return Core.DTOs.ValidationResult.Valid();

            if (limit.Value < 1 || limit.Value > maxLimit)
                return Core.DTOs.ValidationResult.Invalid($"Limit must be between 1 and {maxLimit}");

            return Core.DTOs.ValidationResult.Valid();
        }
    }
}