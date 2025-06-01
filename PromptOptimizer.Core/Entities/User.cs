namespace PromptOptimizer.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public virtual ICollection<ConversationSession> Sessions { get; set; } = new List<ConversationSession>();
    }
}