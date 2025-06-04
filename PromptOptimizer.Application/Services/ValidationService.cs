// PromptOptimizer.Application/Services/ValidationService.cs
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services
{
    public class ValidationService : IValidationService
    {
        public ValidationResult ValidateChatRequest(ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return ValidationResult.Invalid("Message cannot be empty");

            if (request.Message.Length > 5000)
                return ValidationResult.Invalid("Message too long (max 5000 characters)");

            if (string.IsNullOrWhiteSpace(request.Model))
                return ValidationResult.Invalid("Model cannot be empty");

            var validModels = new[]
            {
                "gpt-4o-mini", "gpt-4o", "o3-mini",
                "gemini", "gemini-lite",
                "deepseek-chat", "deepseek-r1",
                "grok-3-mini-beta", "grok-2"
            };

            if (!validModels.Contains(request.Model, StringComparer.OrdinalIgnoreCase))
                return ValidationResult.Invalid($"Invalid model. Valid options: {string.Join(", ", validModels)}");

            if (request.Temperature < 0 || request.Temperature > 2)
                return ValidationResult.Invalid("Temperature must be between 0 and 2");

            if (request.MaxTokens.HasValue && (request.MaxTokens.Value < 1 || request.MaxTokens.Value > 4000))
                return ValidationResult.Invalid("MaxTokens must be between 1 and 4000");

            return ValidationResult.Valid();
        }

        public ValidationResult ValidateStrategyRequest(StrategyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return ValidationResult.Invalid("Message cannot be empty");

            if (request.Message.Length > 5000)
                return ValidationResult.Invalid("Message too long (max 5000 characters)");

            var validStrategies = new[] { "quality", "speed", "cost_effective", "reasoning", "coding", "creative", "default" };
            if (!validStrategies.Contains(request.Strategy?.ToLower()))
                return ValidationResult.Invalid($"Invalid strategy. Valid options: {string.Join(", ", validStrategies)}");

            return ValidationResult.Valid();
        }

        public ValidationResult ValidateSessionId(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return ValidationResult.Invalid("Session ID cannot be empty");

            if (!Guid.TryParse(sessionId, out _) || sessionId.Length != 36)
                return ValidationResult.Invalid("Invalid session ID format");

            return ValidationResult.Valid();
        }

        public ValidationResult ValidateLimit(int? limit, int maxLimit = 100)
        {
            if (!limit.HasValue)
                return ValidationResult.Valid();

            if (limit.Value < 1 || limit.Value > maxLimit)
                return ValidationResult.Invalid($"Limit must be between 1 and {maxLimit}");

            return ValidationResult.Valid();
        }

        public ValidationResult ValidateModel(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return ValidationResult.Invalid("Model name cannot be empty");

            var supportedModels = new[] { "gpt-4o-mini", "gpt-4o", "deepseek-chat", "deepseek-r1" };
            if (!supportedModels.Contains(modelName.ToLower()))
                return ValidationResult.Invalid($"Model '{modelName}' is not supported");

            return ValidationResult.Valid();
        }

        public ValidationResult ValidateTemperature(double temperature)
        {
            if (temperature < 0 || temperature > 2)
                return ValidationResult.Invalid("Temperature must be between 0 and 2");

            return ValidationResult.Valid();
        }
    }
}