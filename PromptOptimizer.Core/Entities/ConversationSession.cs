using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PromptOptimizer.Core.DTOs;

namespace PromptOptimizer.Core.Entities
{
    public class ConversationSession
    {
        [Key]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [StringLength(200)]
        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public string MessagesJson { get; set; } = "[]";

        [NotMapped]
        public List<ConversationMessage> Messages
        {
            get => string.IsNullOrEmpty(MessagesJson)
                ? new List<ConversationMessage>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ConversationMessage>>(MessagesJson) ?? new List<ConversationMessage>();
            set => MessagesJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        public int MessageCount { get; set; } = 0;
        public int MaxMessages { get; set; } = 100;
    }
}