using System.Collections.Generic;

namespace PromptOptimizer.Core.DTOs
{
    public class OptimizationRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string Strategy { get; set; } = "quality";
        public string OptimizationType { get; set; } = "clarity";
        public List<string>? PreferredModels { get; set; }
        public string? SessionId { get; set; }
        public bool EnableMemory { get; set; } = true; 
        public int? ContextWindowSize { get; set; } = 10; 
    }

    public class OptimizationResponse
    {
        public string OriginalPrompt { get; set; } = string.Empty;
        public string OptimizedPrompt { get; set; } = string.Empty;
        public string FinalResponse { get; set; } = string.Empty;
        public List<string> ModelsUsed { get; set; } = new();
        public string Strategy { get; set; } = string.Empty;
        public double ProcessingTimeMs { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}