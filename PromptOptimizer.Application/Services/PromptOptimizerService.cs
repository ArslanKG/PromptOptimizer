using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services;

public class PromptOptimizerService : IPromptOptimizerService
{
    private readonly ICortexApiClient _cortexClient;
    private readonly ILogger<PromptOptimizerService> _logger;

    private readonly Dictionary<string, string> _optimizationPrompts = new()
    {
        ["clarity"] = @"
            Sen bir prompt optimization uzmanısın. Verilen prompt'u daha net, spesifik ve etkili hale getir.
            
            Kurallar:
            1. Belirsizlikleri gider
            2. Spesifik detaylar ekle
            3. Beklenen çıktı formatını belirt
            4. Gerekli bağlamı ekle
            5. Kısa ve öz tut
            
            Optimize edilmiş prompt'u döndür.",

        ["technical"] = @"
            Teknik ve yazılım odaklı promptları optimize et.
            - Programlama dili tercihlerini belirt
            - Teknik gereksinimleri netleştir
            - Kod formatı beklentilerini ekle
            - Best practice'leri dahil et",

        ["creative"] = @"
            Yaratıcı içerik için promptları optimize et.
            - Ton ve stil tercihlerini ekle
            - Yaratıcılık seviyesini belirt
            - Örnek veya referanslar ekle
            - Hedef kitleyi tanımla",

        ["analytical"] = @"
            Analitik ve veri odaklı promptları optimize et.
            - Veri formatı beklentilerini belirt
            - Analiz derinliğini tanımla
            - Metrik ve KPI'ları netleştir
            - Görselleştirme ihtiyaçlarını ekle"
    };

    public PromptOptimizerService(
        ICortexApiClient cortexClient,
        ILogger<PromptOptimizerService> logger)
    {
        _cortexClient = cortexClient;
        _logger = logger;
    }

    public async Task<string> OptimizePromptAsync(
        string originalPrompt,
        string optimizationType = "clarity",
        string model = "gpt-4o-mini",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var systemPrompt = _optimizationPrompts.GetValueOrDefault(optimizationType)
                ?? _optimizationPrompts["clarity"];

            var request = new ChatCompletionRequest
            {
                Model = model,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = $"Optimize this prompt: {originalPrompt}" }
                },
                Temperature = 0.3
            };

            var response = await _cortexClient.CreateChatCompletionAsync(request, cancellationToken);

            return response.Choices.FirstOrDefault()?.Message.Content
                ?? throw new InvalidOperationException("No response from AI model");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to optimize prompt");
            throw;
        }
    }
}