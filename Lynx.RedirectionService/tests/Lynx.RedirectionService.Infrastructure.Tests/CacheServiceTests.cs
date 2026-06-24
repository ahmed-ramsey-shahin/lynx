using FluentAssertions;
using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Infrastructure.Services;
using Lynx.RedirectionService.Infrastructure.Tests.Fixtures;
using StackExchange.Redis;

namespace Lynx.RedirectionService.Infrastructure.Tests
{
    [Collection("CacheCollection")]
    public class CacheServiceTests : IDisposable
    {
        private readonly IDatabase _cache;
        private readonly IConnectionMultiplexer _cluster;
        private readonly ICacheService _cacheService;

        public CacheServiceTests(RedisFixture fixture)
        {
            _cache = fixture.Cluster.GetDatabase();
            _cacheService = new CacheService(fixture.Cluster);
            _cluster = fixture.Cluster;
        }

#region GET_TESTS
        [Fact]
        public async Task GetAsync_Should_ReturnTheSavedObject_WhenGetIsSuccessful()
        {
            // Arrange
            string cacheKey = $"some_random_id{Guid.NewGuid()}";
            Guid cacheValue = Guid.NewGuid();
            await _cacheService.SetAsync(cacheKey, cacheValue);

            // Act
            Guid? result = await _cacheService.GetAsync<Guid?>(cacheKey);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(cacheValue);
        }

        [Fact]
        public async Task GetAsync_Should_ReturnNull_WhenKeyIsNotFound()
        {
            // Arrange
            string cacheKey = $"some_random_id{Guid.NewGuid()}";
            Guid cacheValue = Guid.NewGuid();
            await _cacheService.SetAsync(cacheKey, cacheValue);

            // Act
            Guid? result = await _cacheService.GetAsync<Guid?>($"{cacheKey}_wrong");

            // Assert
            result.Should().BeNull();
        }
#endregion // GET_TESTS

#region SET_TESTS
        [Fact]
        public async Task SetAsync_Should_AddTheValueToRedis()
        {
            // Arrange
            Guid cacheValue = Guid.NewGuid();
            string cacheKey = $"some_random_id{Guid.NewGuid()}";

            // Act
            await _cacheService.SetAsync(cacheKey, cacheValue, TimeSpan.FromMinutes(15));

            // Assert
            RedisValue rawData = await _cache.StringGetAsync(cacheKey);
            rawData.HasValue.Should().BeTrue();
            rawData.ToString().Should().Contain(cacheValue.ToString());
        }

        [Fact]
        public async Task SetAsync_Should_ChangeKeyValueInPlace_IfKeyExists()
        {
            // Arrange
            Guid cacheValue = Guid.NewGuid();
            Guid cacheValue2 = Guid.NewGuid();
            string cacheKey = $"some_random_id{Guid.NewGuid()}";

            // Act
            await _cacheService.SetAsync(cacheKey, cacheValue, TimeSpan.FromMinutes(15));
            await _cacheService.SetAsync(cacheKey, cacheValue2, TimeSpan.FromMinutes(15));

            // Assert
            RedisValue rawData = await _cache.StringGetAsync(cacheKey);
            rawData.HasValue.Should().BeTrue();
            rawData.ToString().Should().Contain(cacheValue2.ToString());
        }

        [Fact]
        public async Task SetAsync_Should_ExpireValueWhenExpirationIsProvided()
        {
            // Arrange
            Guid cacheValue = Guid.NewGuid();
            string cacheKey = $"some_random_id{Guid.NewGuid()}";

            // Act
            await _cacheService.SetAsync(cacheKey, cacheValue, TimeSpan.FromMilliseconds(100));
            await Task.Delay(150);

            // Assert
            var check = await _cache.KeyExistsAsync(cacheKey);
            check.Should().BeFalse();
        }
#endregion // SET_TESTS

        public void Dispose()
        {
            foreach (var endpoint in _cluster.GetEndPoints())
            {
                var server = _cluster.GetServer(endpoint);

                if (server.IsConnected && !server.IsReplica)
                {
                    server.FlushDatabase();
                }
            }
        }
    }
}
