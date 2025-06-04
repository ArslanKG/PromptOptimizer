namespace PromptOptimizer.Core.Interfaces
{
    public interface IRateLimitService
    {
        Task<bool> CheckRateLimitAsync(int userId, string operation = "default");
        Task<RateLimitInfo> GetRateLimitInfoAsync(int userId, string operation = "default");
        
        // Public API için IP bazlı rate limiting
        Task<bool> CheckPublicRateLimitAsync(string ipAddress);
        Task<PublicRateLimitInfo> GetPublicRateLimitInfoAsync(string ipAddress);
    }

    public class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public int Limit { get; set; }
        public TimeSpan ResetTime { get; set; }
        public bool IsLimitExceeded => RequestCount >= Limit;
    }

    public class PublicRateLimitInfo
    {
        public int RequestCount { get; set; }
        public int Limit { get; set; }
        public int RemainingRequests { get; set; }
        public DateTime ResetTime { get; set; }
        public bool IsLimitExceeded => RequestCount >= Limit;
    }
}