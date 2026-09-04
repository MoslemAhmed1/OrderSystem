using Microsoft.Extensions.Options;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Infrastructure.Options;

namespace OrderSystem.Infrastructure.Services
{
    public class CacheVersioningService : ICacheVersioningService
    {
        private readonly ICacheService _cache;
        private readonly TimeSpan _versionKeyExpiration;

        public CacheVersioningService(ICacheService cache, IOptions<CachingOptions> cachingOptions)
        {
            _cache = cache;
            _versionKeyExpiration = TimeSpan.FromDays(cachingOptions.Value.VersionExpirationDays);
        }

        public async Task<int> GetVersionAsync(string versionKey)
        {
            var version = await _cache.GetAsync<int?>(versionKey);
            return version ?? 1;
        }

        public async Task<int> UpdateVersionAsync(string versionKey)
        {
            var current = await GetVersionAsync(versionKey);
            var next = current + 1;
            await _cache.SetAsync(versionKey, next, _versionKeyExpiration, null);
            return next;
        }
    }
}
