using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Infrastructure.Services
{
    public class NullCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            return Task.CompletedTask;
        }
    }
}
