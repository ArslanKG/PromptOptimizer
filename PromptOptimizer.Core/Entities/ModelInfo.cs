namespace PromptOptimizer.Core.Entities;

public class ModelInfo
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;

    public static Dictionary<string, ModelInfo> AvailableModels => new()
    {
        ["gpt-4o-mini"] = new() { Id = "gpt-4o-mini", Type = "fast", Cost = 0.15m, Priority = 1, IsEnabled = true },
        ["gpt-4o"] = new() { Id = "gpt-4o", Type = "advanced", Cost = 1.0m, Priority = 3, IsEnabled = true },
        ["gemini-lite"] = new() { Id = "gemini-lite", Type = "fast", Cost = 0.1m, Priority = 1, IsEnabled = false },
        ["gemini"] = new() { Id = "gemini", Type = "balanced", Cost = 0.5m, Priority = 2, IsEnabled = false },
        ["deepseek-chat"] = new() { Id = "deepseek-chat", Type = "balanced", Cost = 0.3m, Priority = 2, IsEnabled = true, TimeoutSeconds = 60 },
        ["deepseek-r1"] = new() { Id = "deepseek-r1", Type = "reasoning", Cost = 0.8m, Priority = 3, IsEnabled = true, TimeoutSeconds = 60 },
        ["o3-mini"] = new() { Id = "o3-mini", Type = "fast", Cost = 0.2m, Priority = 1, IsEnabled = true },
        ["grok-2"] = new() { Id = "grok-2", Type = "advanced", Cost = 0.9m, Priority = 3, IsEnabled = true },
        ["grok-3-mini-beta"] = new() { Id = "grok-3-mini-beta", Type = "fast", Cost = 0.25m, Priority = 1, IsEnabled = true }
    };

    public static Dictionary<string, ModelInfo> EnabledModels =>
        AvailableModels.Where(m => m.Value.IsEnabled).ToDictionary(m => m.Key, m => m.Value);
}