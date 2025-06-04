using System.ComponentModel.DataAnnotations;

namespace PromptOptimizer.Core.Entities
{
    public class RefreshToken
    {
        [Key]
        public string Token { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRevoked { get; set; } = false;
        
        public DateTime? RevokedAt { get; set; }
        
        public string? ReplacedByToken { get; set; }
        
        // Navigation property
        public virtual User User { get; set; } = null!;
        
        // Helper properties
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}