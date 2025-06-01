using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Core.DTOs;
using PromptOptimizer.Core.Entities;

namespace PromptOptimizer.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // ConversationSession'ı ignore et çünkü in-memory kullanıyoruz
            modelBuilder.Ignore<ConversationSession>();
            modelBuilder.Ignore<ConversationMessage>();
        }
    }
}