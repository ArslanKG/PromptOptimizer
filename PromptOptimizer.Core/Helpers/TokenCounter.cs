using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Helpers
{
    public static class TokenCounter
    {
        public static int EstimateTokens(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var charCount = text.Length;

            return (int)Math.Ceiling(wordCount * 0.75 + charCount * 0.25 / 4.0);
        }

        public static int EstimateMessageTokens(ConversationMessage message)
        {
            return EstimateTokens(message.Role) + EstimateTokens(message.Content) + 4;
        }

        public static int EstimateRequestTokens(List<ChatMessage> messages)
        {
            return messages.Sum(m => EstimateTokens(m.Role) + EstimateTokens(m.Content) + 4) + 10;
        }

        public static string SummarizeOldMessages(List<ConversationMessage> oldMessages)
        {
            if (!oldMessages.Any()) return "";

            var userMessages = oldMessages.Where(m => m.Role == "user").Select(m => m.Content);
            var assistantMessages = oldMessages.Where(m => m.Role == "assistant").Select(m => m.Content);

            var userSummary = string.Join(". ", userMessages.Take(3));
            var assistantSummary = string.Join(". ", assistantMessages.Take(3));

            return $"[Önceki konuşma özeti - Kullanıcı: {userSummary[..Math.Min(200, userSummary.Length)]}... AI: {assistantSummary[..Math.Min(200, assistantSummary.Length)]}...]";
        }
    }
}