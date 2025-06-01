// Core/Configuration/JwtSettings.cs
namespace PromptOptimizer.Core.Configuration
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int TokenExpirationHours { get; set; } = 24;
        public int RefreshTokenExpirationDays { get; set; } = 7;
        public int SlidingExpirationHours { get; set; } = 12;
    }
}