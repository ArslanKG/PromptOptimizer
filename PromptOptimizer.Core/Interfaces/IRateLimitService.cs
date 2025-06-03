namespace PromptOptimizer.Core.Interfaces
{
    public interface IRateLimitService
    {
        Task<bool> CheckRateLimitAsync(int userId, string operation = "default");

        Task<RateLimitInfo> GetRateLimitInfoAsync(int userId, string operation = "default");
    }

    public class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public int Limit { get; set; }
        public TimeSpan ResetTime { get; set; }
        public bool IsLimitExceeded => RequestCount >= Limit;
    }
}