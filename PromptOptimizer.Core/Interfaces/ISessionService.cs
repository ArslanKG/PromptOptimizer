using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;

namespace PromptOptimizer.Core.Interfaces
{
    public interface ISessionService
    {
        Task<ConversationSession> CreateSessionAsync(int? userId = null);
        Task<ConversationSession?> GetSessionAsync(string sessionId);
        Task<ConversationSession> AddMessageAsync(string sessionId, ConversationMessage message);
        Task AddMessageAsync(string sessionId, string role, string content);
        Task<List<ConversationMessage>> GetConversationContextAsync(string sessionId, int? windowSize = null);
        Task<ConversationHistoryResponse> GetConversationHistoryAsync(string sessionId, int? limit = null);
        Task<bool> ClearSessionAsync(string sessionId);
        Task<List<SessionResponse>> GetActiveSessionsAsync(int? userId = null);
        Task<bool> SessionExistsAsync(string sessionId);
    }
}