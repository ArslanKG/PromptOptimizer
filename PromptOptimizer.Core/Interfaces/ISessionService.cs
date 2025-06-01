using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;

namespace PromptOptimizer.Core.Interfaces
{
    public interface ISessionService
    {
        Task<ConversationSession> CreateSessionAsync(string? userId = null);
        Task<ConversationSession?> GetSessionAsync(string sessionId);
        Task<ConversationSession> AddMessageAsync(string sessionId, ConversationMessage message);
        Task<List<ConversationMessage>> GetConversationContextAsync(string sessionId, int? windowSize = null);
        Task<bool> ClearSessionAsync(string sessionId);
        Task<List<SessionResponse>> GetActiveSessionsAsync(string? userId = null);
        Task<ConversationHistoryResponse> GetConversationHistoryAsync(string sessionId, int? limit = null);
    }
}