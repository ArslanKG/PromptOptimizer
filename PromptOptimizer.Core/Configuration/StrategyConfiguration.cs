using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Configuration
{
    public class StrategyConfiguration
    {
        public List<StrategyInfo> Strategies { get; set; } = new();
        public List<OptimizationType> OptimizationTypes { get; set; } = new();

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