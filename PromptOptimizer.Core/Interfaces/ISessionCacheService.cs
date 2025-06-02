using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces
{
    public interface ISessionCacheService
    {
        Task<List<ConversationMessage>> GetSessionMessagesAsync(string sessionId);
        Task AddMessageToSessionAsync(string sessionId, ConversationMessage message);
        Task FlushSessionToDatabaseAsync(string sessionId);
        Task<List<ChatMessage>> GetOptimizedContextAsync(string sessionId, int maxTokens = 4000);
        void ClearSessionCache(string sessionId);
    }
}