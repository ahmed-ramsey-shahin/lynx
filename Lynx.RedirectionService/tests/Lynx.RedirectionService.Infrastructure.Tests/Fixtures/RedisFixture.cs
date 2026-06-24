using StackExchange.Redis;
using Testcontainers.Redis;

namespace Lynx.RedirectionService.Infrastructure.Tests.Fixtures
{
    public class RedisFixture : IAsyncLifetime
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7.2-alpine")
            .Build();
        public IConnectionMultiplexer Cluster { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();
            var connString = _redisContainer.GetConnectionString();
            Cluster = ConnectionMultiplexer.Connect($"{connString},allowAdmin=true");
        }

        public async Task DisposeAsync()
        {
            await _redisContainer.DisposeAsync();
        }
    }

    [CollectionDefinition("CacheCollection")]
    public class RedisCollection : ICollectionFixture<RedisFixture>;
}
