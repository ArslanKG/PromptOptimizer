// PromptOptimizer.Application/Services/SessionTitleGenerator.cs
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace PromptOptimizer.Application.Services
{
    public interface ISessionTitleGenerator
    {
        string GenerateTitle(string firstMessage);
    }

    public class SessionTitleGenerator : ISessionTitleGenerator
    {
        private readonly ILogger<SessionTitleGenerator> _logger;

        private static readonly Dictionary<string, string> TopicPatterns = new()
        {
            { @"\b(python|java|javascript|c#|react|angular|vue)\b", "💻 {0} Programlama" },
            { @"\b(algoritma|veri yapısı|sorting|search)\b", "⚡ Algoritma Soruları" },
            { @"\b(machine learning|ml|ai|yapay zeka)\b", "🤖 Yapay Zeka" },
            { @"\b(web geliştirme|frontend|backend|api)\b", "🌐 Web Geliştirme" },
            { @"\b(database|sql|mongodb|redis)\b", "🗄️ Veritabanı" },
            { @"\b(nasıl öğren|öğrenme|eğitim|kurs)\b", "📚 Öğrenme Rehberi" },
            { @"\b(kariyer|iş|maaş|terfi)\b", "💼 Kariyer Gelişimi" },
            { @"\b(kendimi geliştir|gelişim|beceri)\b", "🚀 Kişisel Gelişim" },
            { @"\b(nedir|ne demek|tanım|açıkla)\b", "❓ Kavram Açıklaması" },
            { @"\b(nasıl yapılır|adım|rehber|guide)\b", "📋 Nasıl Yapılır" },
            { @"\b(örnek|sample|kod|example)\b", "💡 Örnekler" },
            { @"\b(karşılaştır|fark|vs|versus)\b", "⚖️ Karşılaştırma" },
            { @"\b(proje|uygulama|geliştirme)\b", "🛠️ Proje Geliştirme" },
            { @"\b(problem|hata|bug|çözüm)\b", "🔧 Problem Çözme" },
            { @"\b(tavsiye|öneri|suggestion)\b", "💡 Tavsiyeler" }
        };

        public SessionTitleGenerator(ILogger<SessionTitleGenerator> logger)
        {
            _logger = logger;
        }

        public string GenerateTitle(string firstMessage)
        {
            try
            {
                var message = firstMessage?.Trim().ToLowerInvariant() ?? "";

                if (string.IsNullOrEmpty(message))
                {
                    return "💬 Yeni Sohbet";
                }

                foreach (var pattern in TopicPatterns)
                {
                    var match = Regex.Match(message, pattern.Key, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var keyword = ExtractKeyword(message, pattern.Key);
                        var title = string.Format(pattern.Value, keyword);
                        return title;
                    }
                }

                if (IsQuestion(message))
                {
                    var questionTitle = GenerateQuestionTitle(firstMessage);
                    if (!string.IsNullOrEmpty(questionTitle))
                    {
                        return $"❓ {questionTitle}";
                    }
                }

                return GenerateFallbackTitle(firstMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Title generation failed for message: '{Message}'", firstMessage);
                return "💬 Yeni Sohbet";
            }
        }

        private string ExtractKeyword(string message, string pattern)
        {
            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
            return match.Success ? TitleCase(match.Value) : "";
        }

        private bool IsQuestion(string message)
        {
            return message.Contains('?') ||
                   Regex.IsMatch(message, @"\b(nasıl|nedir|kim|neden|ne|hangi|kaç)\b", RegexOptions.IgnoreCase);
        }

        private string GenerateQuestionTitle(string message)
        {
            var cleanMessage = message.Replace("merhaba", "").Replace("selam", "").Trim();

            if (cleanMessage.Length <= 3)
                return "";

            var questionPart = cleanMessage.Length > 40
                ? cleanMessage[..37] + "..."
                : cleanMessage;

            return TitleCase(questionPart);
        }

        private string GenerateFallbackTitle(string message)
        {
            var cleanMessage = Regex.Replace(message,
                @"\b(merhaba|selam|hello|hi|hey|selamlar)\b", "",
                RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrEmpty(cleanMessage))
                return "💬 Yeni Sohbet";

            var title = cleanMessage.Length > 30
                ? cleanMessage[..27] + "..."
                : cleanMessage;

            return $"💬 {TitleCase(title)}";
        }

        private string TitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var titleCased = words.Select(word =>
                char.ToUpper(word[0]) + (word.Length > 1 ? word[1..].ToLower() : ""));

            return string.Join(" ", titleCased);
        }
    }
}