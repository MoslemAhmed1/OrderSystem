using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;

using OrderSystem.Infrastructure.Options;
using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly TimeSpan _defaultExpiration;
        private readonly TimeSpan _slidingExpiration;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache distributedCache, IOptions<CachingOptions> cachingOptions, ILogger<RedisCacheService> logger) // TODO: IOptionsMonitor ??
        {
            _distributedCache = distributedCache;
            _defaultExpiration = TimeSpan.FromMinutes(cachingOptions.Value.DefaultExpirationMinutes);
            _slidingExpiration = TimeSpan.FromMinutes(cachingOptions.Value.SlidingExpirationMinutes);
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cachedData = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedData))
                    return default;

                try
                {
                    return JsonSerializer.Deserialize<T>(cachedData);
                }
                catch (JsonException)
                {
                    return default;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Redis GET failed for key '{key}'");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultExpiration,
                    SlidingExpiration = slidingExpiration ?? _slidingExpiration
                };

                var jsonData = JsonSerializer.Serialize(value);
                await _distributedCache.SetStringAsync(key, jsonData, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Redis SET failed for key '{key}'");
                return; 
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Redis REMOVE failed for key '{key}'");
                return;
            }
        }
    }
}
