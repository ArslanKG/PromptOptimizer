// PromptOptimizer.Core/DTOs/ChatModels.cs
namespace PromptOptimizer.Core.DTOs
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini";
        public string? SessionId { get; set; }
        public double Temperature { get; set; } = 0.7;
        public int? MaxTokens { get; set; }
    }

    public class ChatResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Usage? Usage { get; set; }
        public bool Success { get; set; } = true;
        public string? SessionTitle { get; set; }
        public bool IsNewSession { get; set; } = false;
    }

    public class StrategyRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Strategy { get; set; } = "default";
        public string? SessionId { get; set; }
    }

    public class ModelListResponse
    {
        public List<AvailableModel> Models { get; set; } = new();
    }

    public class AvailableModel
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public decimal CostPer1KTokens { get; set; }
        public bool IsRecommended { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ErrorResponse(string message, string? details = null)
        {
            Message = message;
            Details = details;
        }
    }

    public class ModelConfig
    {
        public string Name { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
    }
}