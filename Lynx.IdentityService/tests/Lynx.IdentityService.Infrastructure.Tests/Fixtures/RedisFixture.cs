using StackExchange.Redis;
using Testcontainers.Redis;

namespace Lynx.IdentityService.Infrastructure.Tests.Fixtures
{
    public class RedisFixture : IAsyncLifetime
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7.2-alpine")
            .Build();
        public IConnectionMultiplexer Cluster { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();
            Cluster = ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
        }

        public async Task DisposeAsync()
        {
            await _redisContainer.DisposeAsync();
        }
    }

    [CollectionDefinition("CacheCollection")]
    public class RedisCollection : ICollectionFixture<RedisFixture>;
}
