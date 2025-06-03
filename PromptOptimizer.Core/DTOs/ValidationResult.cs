namespace PromptOptimizer.Core.DTOs
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Valid() => new(true);

        public static ValidationResult Invalid(string errorMessage) => new(false, errorMessage);
    }
}