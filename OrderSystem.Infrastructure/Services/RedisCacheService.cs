using System.Text.Json;

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
        private readonly NullCacheService _nullCacheService;

        public RedisCacheService(IDistributedCache distributedCache, IOptions<CachingOptions> cachingOptions)
        {
            _distributedCache = distributedCache;
            _defaultExpiration = TimeSpan.FromMinutes(cachingOptions.Value.DefaultExpirationMinutes);
            _slidingExpiration = TimeSpan.FromMinutes(cachingOptions.Value.SlidingExpirationMinutes);
            _nullCacheService = new NullCacheService();
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
            catch (Exception)
            {
                return await _nullCacheService.GetAsync<T>(key);
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
            catch (Exception)
            {
                await _nullCacheService.SetAsync(key, value, absoluteExpiration, slidingExpiration);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception)
            {
                await _nullCacheService.RemoveAsync(key);
            }
        }
    }
}
