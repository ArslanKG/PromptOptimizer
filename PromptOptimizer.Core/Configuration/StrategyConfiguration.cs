using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Configuration
{
    public class StrategyConfiguration
    {
        public List<StrategyInfo> Strategies { get; set; } = new();
        public List<OptimizationType> OptimizationTypes { get; set; } = new();
        public Dictionary<string, ModelConfig> ModelStrategies { get; set; } = new();

        public static StrategyConfiguration GetDefault()
        {
            return new StrategyConfiguration
            {
                Strategies = new List<StrategyInfo>
                {
                    new() {
                        Id = "quality",
                        Name = "En İyi Kalite",
                        Description = "Çoklu model kullanarak en kaliteli cevap",
                        EstimatedTime = "15-20 saniye"
                    },
                    new() {
                        Id = "speed",
                        Name = "Hızlı",
                        Description = "Hızlı modeller ile quick response",
                        EstimatedTime = "3-5 saniye"
                    },
                    new() {
                        Id = "consensus",
                        Name = "Konsensüs",
                        Description = "Birden fazla model görüşü",
                        EstimatedTime = "20-30 saniye"
                    },
                    new() {
                        Id = "cost_effective",
                        Name = "Maliyet Odaklı",
                        Description = "Uygun maliyetli çözüm",
                        EstimatedTime = "5-10 saniye"
                    }
                },
                ModelStrategies = new Dictionary<string, ModelConfig>
                {
                    ["quality"] = new() { Name = "gpt-4o", Temperature = 0.7, MaxTokens = 2000 },
                    ["speed"] = new() { Name = "gpt-4o-mini", Temperature = 0.5, MaxTokens = 1000 },
                    ["cost_effective"] = new() { Name = "gpt-4o-mini", Temperature = 0.7, MaxTokens = 1500 },
                    ["reasoning"] = new() { Name = "deepseek-r1", Temperature = 0.3, MaxTokens = 2000 },
                    ["creative"] = new() { Name = "grok-2", Temperature = 0.8, MaxTokens = 1800 },
                    ["consensus"] = new() { Name = "gpt-4o", Temperature = 0.6, MaxTokens = 1800 },
                    ["default"] = new() { Name = "gpt-4o-mini", Temperature = 0.5, MaxTokens = 1500 }
                },
                OptimizationTypes = new List<OptimizationType>
                {
                    new() {
                        Id = "clarity",
                        Name = "Netlik",
                        Description = "Belirsizlikleri giderir ve daha net hale getirir"
                    },
                    new() {
                        Id = "technical",
                        Name = "Teknik",
                        Description = "Teknik ve yazılım odaklı optimize eder"
                    },
                    new() {
                        Id = "creative",
                        Name = "Yaratıcı",
                        Description = "Yaratıcı içerik için optimize eder"
                    },
                    new() {
                        Id = "analytical",
                        Name = "Analitik",
                        Description = "Analitik ve veri odaklı optimize eder"
                    }
                }
            };
        }
    }
}