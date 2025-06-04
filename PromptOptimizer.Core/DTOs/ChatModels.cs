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
        public string ErrorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ErrorResponse(string errorCode, string message, string? details = null)
        {
            ErrorCode = errorCode;
            Message = message;
            Details = details;
        }

        // Backward compatibility constructor
        public ErrorResponse(string message, string? details = null)
        {
            ErrorCode = "GENERIC_ERROR";
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

    // Public API DTOs - Auth olmadan kullanım için
    public class PublicChatRequest
    {
        public string Message { get; set; } = string.Empty;
        // Model fixed: sadece gpt-4o-mini
        // Session yok: memory tutmuyor
        // Temperature fixed: 0.7 (cost-effective)
        // MaxTokens fixed: 1000 (cost-effective)
    }

    public class PublicChatResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Usage? Usage { get; set; }
        public bool Success { get; set; } = true;
        public int RemainingRequests { get; set; }
        public DateTime ResetTime { get; set; }
    }
}