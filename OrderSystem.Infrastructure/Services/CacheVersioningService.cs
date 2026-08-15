using OrderSystem.Application.Interfaces.Services;
namespace OrderSystem.Infrastructure.Services
{
    public class CacheVersioningService : ICacheVersioningService
    {
        private readonly ICacheService _cache;
        private readonly TimeSpan _versionKeyExpiration = TimeSpan.FromDays(30); // TODO: move to configuration

        public CacheVersioningService(ICacheService cache)
        {
            _cache = cache;
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
