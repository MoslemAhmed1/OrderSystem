using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

using OrderSystem.Infrastructure.Options;
using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Infrastructure.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _defaultExpiration;
        private readonly TimeSpan _slidingExpiration;

        public InMemoryCacheService(IMemoryCache memoryCache, IOptions<CachingOptions> cachingOptions) // TODO: IOptionsMonitor ??
        {
            _memoryCache = memoryCache;
            _defaultExpiration = TimeSpan.FromMinutes(cachingOptions.Value.DefaultExpirationMinutes);
            _slidingExpiration = TimeSpan.FromMinutes(cachingOptions.Value.SlidingExpirationMinutes);
        }

        public Task<T?> GetAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultExpiration,
                SlidingExpiration = slidingExpiration ?? _slidingExpiration
            };
            _memoryCache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
