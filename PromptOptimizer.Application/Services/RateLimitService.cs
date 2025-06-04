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
                "public_chat" => 30, // Saatlik 30 request public API için
                _ => 50
            };
        }

        // Public API için IP bazlı rate limiting
        public Task<bool> CheckPublicRateLimitAsync(string ipAddress)
        {
            var currentHour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
            var key = $"public_rate_limit_{ipAddress}_{currentHour}";
            
            const int hourlyLimit = 30; // Saatlik limit

            if (_cache.TryGetValue(key, out int requestCount))
            {
                if (requestCount >= hourlyLimit)
                {
                    _logger.LogWarning("Public API rate limit exceeded for IP {IpAddress}", ipAddress);
                    return Task.FromResult(false);
                }
                _cache.Set(key, requestCount + 1, TimeSpan.FromHours(1));
            }
            else
            {
                _cache.Set(key, 1, TimeSpan.FromHours(1));
            }

            _logger.LogDebug("Public API request {Count}/{Limit} for IP {IpAddress}",
                requestCount + 1, hourlyLimit, ipAddress);

            return Task.FromResult(true);
        }

        // Public API için rate limit bilgisi
        public Task<PublicRateLimitInfo> GetPublicRateLimitInfoAsync(string ipAddress)
        {
            var currentHour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
            var key = $"public_rate_limit_{ipAddress}_{currentHour}";
            
            const int hourlyLimit = 30;
            var requestCount = _cache.TryGetValue(key, out int count) ? count : 0;
            var remainingRequests = Math.Max(0, hourlyLimit - requestCount);
            
            // Bir sonraki saatin başını hesapla
            var nextHour = DateTime.UtcNow.AddHours(1);
            var resetTime = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 0, DateTimeKind.Utc);

            return Task.FromResult(new PublicRateLimitInfo
            {
                RequestCount = requestCount,
                Limit = hourlyLimit,
                RemainingRequests = remainingRequests,
                ResetTime = resetTime
            });
        }
    }
}