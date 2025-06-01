namespace PromptOptimizer.Core.DTOs;

public class ModelTestResponse
{
    public string Model { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public Usage? Usage { get; set; }
}