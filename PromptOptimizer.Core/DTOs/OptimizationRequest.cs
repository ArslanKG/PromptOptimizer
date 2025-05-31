namespace PromptOptimizer.Core.DTOs;

public class OptimizationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string Strategy { get; set; } = "quality"; // quality, speed, consensus, cost_effective
    public string OptimizationType { get; set; } = "clarity"; // clarity, technical, creative, analytical
    public List<string>? PreferredModels { get; set; }
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