using System;
using System.Collections.Generic;
using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Entities
{
    public class ConversationSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
        public List<ConversationMessage> Messages { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public int MaxMessages { get; set; } = 20;
    }
}