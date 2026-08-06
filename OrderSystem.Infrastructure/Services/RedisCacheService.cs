using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using OrderSystem.Application.Caching;
using OrderSystem.Application.Interfaces.Services;
using System.Text.Json;

namespace OrderSystem.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly TimeSpan _defaultExpiration;
        private readonly TimeSpan _slidingExpiration;

        public RedisCacheService(IDistributedCache distributedCache, IOptions<CachingOptions> cachingOptions)
        {
            _distributedCache = distributedCache;
            _defaultExpiration = TimeSpan.FromMinutes(cachingOptions.Value.DefaultExpirationMinutes);
            _slidingExpiration = TimeSpan.FromMinutes(cachingOptions.Value.SlidingExpirationMinutes);
        }

        public async Task<T?> GetAsync<T>(string key)
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

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultExpiration,
                SlidingExpiration = slidingExpiration ?? _slidingExpiration
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, jsonData, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
}
