using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services
{
    public class PromptOptimizerService : IPromptOptimizerService
    {
        private readonly ICortexApiClient _cortexClient;
        private readonly ILogger<PromptOptimizerService> _logger;

        public PromptOptimizerService(ICortexApiClient cortexClient, ILogger<PromptOptimizerService> logger)
        {
            _cortexClient = cortexClient;
            _logger = logger;
        }

        public async Task<string> OptimizePromptAsync(
            string prompt,
            string optimizationType,
            string model,
            List<ConversationMessage>? context = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting optimization - Prompt: '{Prompt}', Type: {Type}, Context: {HasContext}",
                    prompt, optimizationType, context?.Count ?? 0);

                if (IsPromptAlreadyOptimal(prompt, context))
                {
                    _logger.LogInformation("Prompt marked as already optimal, returning unchanged");
                    return prompt;
                }

                var contextTopic = await ExtractTopicFromContextAsync(context, model, cancellationToken);
                var optimizationPrompt = BuildMinimalOptimizationPrompt(prompt, optimizationType, contextTopic);
                var temperature = GetOptimalTemperature(optimizationType);

                _logger.LogDebug("Sending optimization request - Temperature: {Temp}, Topic: '{Topic}', OptPrompt: '{OptPrompt}'",
                    temperature, contextTopic, optimizationPrompt);

                var request = new ChatCompletionRequest
                {
                    Model = model,
                    Messages = new List<ChatMessage>
                    {
                        new() { Role = "user", Content = optimizationPrompt }
                    },
                    Temperature = temperature,
                    MaxTokens = 100
                };

                var response = await _cortexClient.CreateChatCompletionAsync(request, cancellationToken);
                var rawOptimized = response.Choices?.FirstOrDefault()?.Message?.Content;

                _logger.LogDebug("Raw optimization response: '{Raw}'", rawOptimized);

                var optimizedPrompt = CleanAndValidatePrompt(rawOptimized, prompt);

                _logger.LogInformation("Optimization completed - Original: '{Original}' → Optimized: '{Optimized}'",
                    prompt, optimizedPrompt);

                return optimizedPrompt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Optimization failed for prompt: '{Prompt}', using original", prompt);
                return prompt;
            }
        }

        public async Task<string> OptimizePromptWithSessionAsync(
            string prompt,
            string optimizationType,
            string model,
            string sessionId,
            ISessionCacheService sessionCacheService,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("OptimizeWithSession - SessionId: {SessionId}, Prompt: '{Prompt}'",
                    sessionId, prompt);

                var sessionMessages = await sessionCacheService.GetSessionMessagesAsync(sessionId);
                var recentContext = sessionMessages
                    .OrderBy(m => m.Timestamp)
                    .TakeLast(4)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} context messages from session", recentContext.Count);

                // Debug: context'i logla
                foreach (var msg in recentContext)
                {
                    _logger.LogDebug("Context - Role: {Role}, Content: '{Content}'",
                        msg.Role, msg.Content[..Math.Min(50, msg.Content.Length)] + "...");
                }

                return await OptimizePromptAsync(prompt, optimizationType, model, recentContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OptimizePromptWithSessionAsync for session {SessionId}", sessionId);
                return prompt;
            }
        }

        private bool IsPromptAlreadyOptimal(string prompt, List<ConversationMessage>? context)
        {
            // 1. Çok uzun promptlar zaten detaylı
            if (prompt.Length > 100)
            {
                _logger.LogDebug("Prompt too long ({Length} chars), skipping optimization", prompt.Length);
                return true;
            }

            // 2. Belirsiz terimler yoksa optimization gereksiz
            var hasVagueTerms = ContainsVagueTerms(prompt);
            if (!hasVagueTerms)
            {
                _logger.LogDebug("No vague terms detected in '{Prompt}', skipping optimization", prompt);
                return true;
            }

            // 3. DÜZELTME: Context varsa optimization YAP, yoksa da YAP (basic)
            if (context == null || !context.Any())
            {
                _logger.LogDebug("No context available for '{Prompt}', but has vague terms - will optimize", prompt);
                return false; // Context yoksa da optimize et
            }

            _logger.LogDebug("Context available ({Count} messages) and vague terms detected - will optimize", context.Count);
            return false; // Context var ve belirsiz terimler var, kesinlikle optimize et
        }

        private bool ContainsVagueTerms(string prompt)
        {
            var lowerPrompt = prompt.ToLower();

            var vaguePatterns = new[]
            {
                @"\b(avantaj|dezavantaj|özellik|nasıl|nedir|nelerdir|örnekleri?|faydalar?ı?|zararlar?ı?)\b",
                @"\b(bunun?|onun?|şunun?|bu konuda|hakkında)\b",
                @"\b(daha|en iyi|hangi|ne|kim|neden)\s+\w*\s*\??"
            };

            foreach (var pattern in vaguePatterns)
            {
                if (Regex.IsMatch(lowerPrompt, pattern))
                {
                    _logger.LogDebug("Vague term pattern matched: '{Pattern}' in '{Prompt}'", pattern, prompt);
                    return true;
                }
            }

            _logger.LogDebug("No vague terms found in '{Prompt}'", prompt);
            return false;
        }

        private async Task<string> ExtractTopicFromContextAsync(
            List<ConversationMessage>? context,
            string model,
            CancellationToken cancellationToken)
        {
            if (context == null || !context.Any())
            {
                _logger.LogDebug("No context available for topic extraction");
                return "";
            }

            var userMessages = context
                .Where(m => m.Role == "user")
                .Select(m => m.Content)
                .ToList();

            if (!userMessages.Any())
            {
                _logger.LogDebug("No user messages found in context");
                return "";
            }

            var contextText = string.Join(" ", userMessages);
            _logger.LogDebug("Context text for topic extraction: '{Context}'",
                contextText[..Math.Min(100, contextText.Length)] + "...");

            if (contextText.Length < 10)
            {
                _logger.LogDebug("Context too short for extraction");
                return "";
            }

            try
            {
                var extractionPrompt = $@"Şu konuşmadaki ana konuyu tek kelime veya kısa cümle ile söyle:

Konuşma: ""{contextText[..Math.Min(300, contextText.Length)]}""

Ana konu:";

                _logger.LogDebug("Topic extraction prompt: '{Prompt}'", extractionPrompt);

                var request = new ChatCompletionRequest
                {
                    Model = model,
                    Messages = new List<ChatMessage>
                    {
                        new() { Role = "user", Content = extractionPrompt }
                    },
                    Temperature = 0.1,
                    MaxTokens = 30
                };

                var response = await _cortexClient.CreateChatCompletionAsync(request, cancellationToken);
                var topic = response.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";

                var cleanedTopic = CleanTopic(topic);
                _logger.LogInformation("Extracted topic: '{Topic}' from context", cleanedTopic);

                return cleanedTopic;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI topic extraction failed, using regex fallback");
                return ExtractTopicWithRegex(contextText);
            }
        }

        private string ExtractTopicWithRegex(string contextText)
        {
            _logger.LogDebug("Using regex topic extraction for: '{Context}'",
                contextText[..Math.Min(50, contextText.Length)] + "...");

            var techPatterns = new Dictionary<string, string>
            {
                { @"\bpython\b", "Python" },
                { @"\bjavascript\b|\bjs\b", "JavaScript" },
                { @"\breact\b", "React" },
                { @"\bnode\b", "Node.js" },
                { @"\bapi\b", "API" },
                { @"\bdatabase\b|\bdb\b", "Database" },
                { @"\bai\b|\byapay zeka\b", "AI" },
                { @"\bmachine learning\b|\bml\b", "ML" },
                { @"\bdocker\b", "Docker" },
                { @"\bkubernetes\b|\bk8s\b", "Kubernetes" }
            };

            foreach (var pattern in techPatterns)
            {
                if (Regex.IsMatch(contextText.ToLower(), pattern.Key))
                {
                    _logger.LogDebug("Regex matched topic: '{Topic}' with pattern '{Pattern}'",
                        pattern.Value, pattern.Key);
                    return pattern.Value;
                }
            }

            var words = contextText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var importantWord = words
                .Where(w => w.Length > 4 && !IsCommonWord(w.ToLower()))
                .FirstOrDefault();

            var result = importantWord ?? "";
            _logger.LogDebug("Regex extraction result: '{Result}'", result);
            return result;
        }

        private bool IsCommonWord(string word)
        {
            var commonWords = new[] { "nedir", "nasıl", "hangi", "avantaj", "dezavantaj", "özellik", "örnek", "kullanım", "çalışır", "yapar" };
            return commonWords.Contains(word);
        }

        private string CleanTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return "";

            topic = Regex.Replace(topic, @"[^\w\s\-\.]", "").Trim();
            var cleaned = topic.Length > 30 ? topic[..30] : topic;

            _logger.LogDebug("Topic cleaned: '{Original}' → '{Cleaned}'", topic, cleaned);
            return cleaned;
        }

        private string BuildMinimalOptimizationPrompt(string prompt, string optimizationType, string contextTopic)
        {
            var baseInstruction = optimizationType.ToLower() switch
            {
                "clarity" => "Bu belirsiz soruyu net hale getir",
                "technical" => "Bu soruyu teknik detaylandır",
                "creative" => "Bu soruyu yaratıcı genişlet",
                "analytical" => "Bu soruyu analitik yapılandır",
                _ => "Bu soruyu iyileştir"
            };

            string optimizationPrompt;
            if (!string.IsNullOrEmpty(contextTopic))
            {
                optimizationPrompt = $@"{baseInstruction}. Eğer belirsiz bir soru soruyorsa, {contextTopic} konusuyla ilişkilendir.

Orijinal soru: ""{prompt}""

İyileştirilmiş soru:";
            }
            else
            {
                optimizationPrompt = $@"{baseInstruction}.

Orijinal soru: ""{prompt}""

İyileştirilmiş soru:";
            }

            _logger.LogDebug("Built optimization prompt with topic '{Topic}': '{OptPrompt}'",
                contextTopic, optimizationPrompt);
            return optimizationPrompt;
        }

        private double GetOptimalTemperature(string optimizationType)
        {
            var temp = optimizationType.ToLower() switch
            {
                "clarity" => 0.1,      // En düşük - net ve tutarlı
                "technical" => 0.2,    // Düşük - kesin ve spesifik
                "analytical" => 0.3,   // Orta-düşük - yapılandırılmış
                "creative" => 0.7,     // Yüksek - yaratıcı ve çeşitli
                _ => 0.4               // Orta - dengeli
            };

            _logger.LogDebug("Temperature for '{Type}': {Temp}", optimizationType, temp);
            return temp;
        }

        private string CleanAndValidatePrompt(string? optimizedPrompt, string originalPrompt)
        {
            if (string.IsNullOrWhiteSpace(optimizedPrompt))
            {
                _logger.LogWarning("Empty optimization result, returning original");
                return originalPrompt;
            }

            optimizedPrompt = optimizedPrompt.Trim();

            // Remove quotes
            if ((optimizedPrompt.StartsWith("\"") && optimizedPrompt.EndsWith("\"")) ||
                (optimizedPrompt.StartsWith("'") && optimizedPrompt.EndsWith("'")))
            {
                optimizedPrompt = optimizedPrompt[1..^1].Trim();
                _logger.LogDebug("Removed quotes from optimization result");
            }

            var prefixes = new[] {
                "iyileştirilmiş soru:", "net soru:", "düzeltilmiş:", "geliştirilmiş:",
                "optimized:", "improved:", "better:", "iyileştir:", "net hale getir:",
                "teknik detaylandır:", "yaratıcı genişlet:", "analitik yapılandır:"
            };

            foreach (var prefix in prefixes)
            {
                if (optimizedPrompt.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    optimizedPrompt = optimizedPrompt[prefix.Length..].Trim();
                    _logger.LogDebug("Removed prefix '{Prefix}' from result", prefix);
                    break;
                }
            }

            // Validation
            if (optimizedPrompt.Length < 3 || optimizedPrompt.Length > 300)
            {
                _logger.LogWarning("Invalid optimization length ({Length}), returning original", optimizedPrompt.Length);
                return originalPrompt;
            }

            if (optimizedPrompt.Split(' ').Length < 2)
            {
                _logger.LogWarning("Optimization too short (word count), returning original");
                return originalPrompt;
            }

            _logger.LogInformation("Validation successful - Final result: '{Result}'", optimizedPrompt);
            return optimizedPrompt;
        }
    }
}