namespace PromptOptimizer.Core.DTOs
{
    public class StrategyInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
    }

    public class OptimizationType
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}