using Lynx.IdentityService.Application.Common.Services;

namespace Lynx.IdentityService.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> TrySetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
