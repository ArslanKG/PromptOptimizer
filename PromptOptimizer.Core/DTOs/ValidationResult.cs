namespace PromptOptimizer.Core.DTOs
{
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public List<string> Errors { get; private set; } = new();

        private ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Errors.Add(errorMessage);
            }
        }

        public static ValidationResult Valid() => new(true);

        public static ValidationResult Invalid(string errorMessage) => new(false, errorMessage);

        public static ValidationResult Invalid(List<string> errors) => new(false)
        {
            Errors = errors,
            ErrorMessage = string.Join("; ", errors)
        };

        // Helper method for multiple validations
        public ValidationResult Combine(ValidationResult other)
        {
            if (IsValid && other.IsValid)
                return Valid();

            var allErrors = new List<string>();
            allErrors.AddRange(Errors);
            allErrors.AddRange(other.Errors);

            return Invalid(allErrors);
        }
    }
}