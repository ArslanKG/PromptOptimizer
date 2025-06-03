using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Core.Interfaces;

namespace PromptOptimizer.Application.Services
{
    public class RateLimitService : IRateLimitService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitService> _logger;

        public RateLimitService(IMemoryCache cache, ILogger<RateLimitService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<bool> CheckRateLimitAsync(int userId, string operation = "default")
        {
            var rateLimitKey = $"rate_limit_{userId}_{operation}";
            var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
            var key = $"{rateLimitKey}_{currentMinute}";

            var limit = GetLimitForOperation(operation);

            if (_cache.TryGetValue(key, out int requestCount))
            {
                if (requestCount >= limit)
                {
                    _logger.LogWarning("Rate limit exceeded for user {UserId}, operation {Operation}", userId, operation);
                    return Task.FromResult(false);
                }
                _cache.Set(key, requestCount + 1, TimeSpan.FromMinutes(1));
            }
            else
            {
                _cache.Set(key, 1, TimeSpan.FromMinutes(1));
            }

            return Task.FromResult(true);
        }

        public Task<RateLimitInfo> GetRateLimitInfoAsync(int userId, string operation = "default")
        {
            var rateLimitKey = $"rate_limit_{userId}_{operation}";
            var currentMinute = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm");
            var key = $"{rateLimitKey}_{currentMinute}";

            var limit = GetLimitForOperation(operation);
            var requestCount = _cache.TryGetValue(key, out int count) ? count : 0;

            return Task.FromResult(new RateLimitInfo
            {
                RequestCount = requestCount,
                Limit = limit,
                ResetTime = TimeSpan.FromMinutes(1)
            });
        }

        private static int GetLimitForOperation(string operation)
        {
            return operation.ToLower() switch
            {
                "optimize" => 60,
                "session" => 120,
                "default" => 100,
                _ => 50
            };
        }
    }
}