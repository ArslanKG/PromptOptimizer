using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Extensions
{
    public static class ValidationExtensions
    {
        public static ValidationResult Combine(this ValidationResult first, ValidationResult second)
        {
            if (first.IsValid && second.IsValid)
                return ValidationResult.Valid();

            var errors = new List<string>();
            if (!first.IsValid) errors.AddRange(first.Errors);
            if (!second.IsValid) errors.AddRange(second.Errors);

            return ValidationResult.Invalid(errors);
        }

        public static ValidationResult CombineAll(params ValidationResult[] results)
        {
            var validResults = results.Where(r => r.IsValid).ToList();
            if (validResults.Count == results.Length)
                return ValidationResult.Valid();

            var allErrors = results
                .Where(r => !r.IsValid)
                .SelectMany(r => r.Errors)
                .ToList();

            return ValidationResult.Invalid(allErrors);
        }
    }
}