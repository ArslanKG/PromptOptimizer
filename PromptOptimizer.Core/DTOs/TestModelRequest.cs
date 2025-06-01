namespace PromptOptimizer.Core.DTOs
{
    public class TestModelRequest
    {
        public string Model { get; set; } = "gpt-4o-mini";
        public string Prompt { get; set; } = string.Empty;
    }
}