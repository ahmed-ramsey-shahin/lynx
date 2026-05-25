using StackExchange.Redis;
using Testcontainers.Redis;

namespace Lynx.IdentityService.Infrastructure.Tests.Fixtures
{
    public class RedisFixture
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis-alpine")
            .Build();
        public IConnectionMultiplexer RedisConnection { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();
            RedisConnection = ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
        }

        public async Task DisposeAsync()
        {
            await _redisContainer.DisposeAsync();
        }
    }

    [CollectionDefinition("CacheCollection")]
    public class RedisCollection : ICollectionFixture<RedisFixture>;
}
