namespace PromptOptimizer.Core.DTOs
{
    public class SessionResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MessageCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastActivityAt { get; set; }
        public string Title { get; set; } = string.Empty;

    }

    public class ConversationHistoryResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public List<ConversationMessage> Messages { get; set; } = new();
        public int TotalMessages { get; set; }
        public DateTime? LastActivityAt { get; set; }
    }

    public class CreateSessionRequest
    {
        public string? UserId { get; set; }
    }

    public class MessageDto
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

}