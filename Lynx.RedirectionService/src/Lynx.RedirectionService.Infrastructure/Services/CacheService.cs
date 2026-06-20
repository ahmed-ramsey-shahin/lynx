using System.Text.Json;
using Lynx.RedirectionService.Application.Common.Services;
using StackExchange.Redis;

namespace Lynx.RedirectionService.Infrastructure.Services
{
    public class CacheService(IConnectionMultiplexer cluster) : ICacheService
    {
        private readonly IDatabase _cache = cluster.GetDatabase();

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var result = await _cache.StringGetAsync(key);

            if (!result.HasValue)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(result.ToString());
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(value);
            Expiration redisExpiration = expiration.HasValue
                ? new Expiration(expiration.Value)
                : default;
            await _cache.StringSetAsync(key, json, redisExpiration);
        }

        public async Task<bool> TrySetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(value);
            Expiration redisExpiration = expiration.HasValue
                ? new Expiration(expiration.Value)
                : default;
            return await _cache.StringSetAsync(
                key,
                json,
                redisExpiration,
                When.NotExists
            );
        }
    }
}
