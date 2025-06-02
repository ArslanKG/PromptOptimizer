using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Interfaces
{
    public interface ISessionManagementService
    {
        Task<SessionOperationResult<SessionResponse>> GetSessionAsync(string sessionId, int userId);
        Task<SessionOperationResult<ConversationHistoryResponse>> GetHistoryAsync(string sessionId, int userId, int? limit = null);
        Task<SessionOperationResult<bool>> ClearSessionAsync(string sessionId, int userId);
        Task<SessionOperationResult<List<SessionResponse>>> GetUserSessionsAsync(int userId, int? limit = null);
        Task<bool> ValidateSessionAccessAsync(string sessionId, int userId);
    }
}