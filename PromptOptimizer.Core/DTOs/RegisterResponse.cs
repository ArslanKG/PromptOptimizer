namespace PromptOptimizer.Core.DTOs
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}