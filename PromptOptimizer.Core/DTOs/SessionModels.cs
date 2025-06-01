using System;
using System.Collections.Generic;

namespace PromptOptimizer.Core.DTOs
{
    // Session related classes
    public class SessionResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MessageCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class ConversationHistoryResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public List<ConversationMessage> Messages { get; set; } = new();
        public int TotalMessages { get; set; }
        public DateTime LastActivityAt { get; set; }
    }

    public class ConversationMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Model { get; set; }
        public int? TokenCount { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class CreateSessionRequest
    {
        public string? UserId { get; set; }
    }

}