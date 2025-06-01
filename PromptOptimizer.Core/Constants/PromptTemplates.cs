namespace PromptOptimizer.Core.Constants;

public static class PromptTemplates
{
    public const string SpeedSystemPrompt = "Kısa, net ve hızlı cevaplar ver.";
    public const string OptimizePromptPrefix = "Optimize this prompt: ";

    public const string ConsensusPromptTemplate = @"
        Aşağıdaki {0} farklı AI modelinden gelen cevapları analiz et ve en iyi kısımları birleştirerek tek bir kapsamlı cevap oluştur:
        {1}
        Birleştirilmiş cevap net, tutarlı ve tüm modellerin güçlü yanlarını içermelidir.";

    public const string ModelResponseTemplate = "Model {0} Cevabı:\n{1}";
}