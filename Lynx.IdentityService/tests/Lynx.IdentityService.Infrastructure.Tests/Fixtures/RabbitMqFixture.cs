using Lynx.IdentityService.Infrastructure.Services.RabbitMq;
using Testcontainers.RabbitMq;

namespace Lynx.IdentityService.Infrastructure.Tests.Fixtures
{
    public class RabbitMqFixture : IAsyncLifetime
    {
        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:3-management").Build();
        private IRabbitMqConnectionManager _connManager = null!;
        public IRabbitMqChannelPool ChannelPool { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _rabbitMqContainer.StartAsync();
            _connManager = new RabbitMqConnectionManager(_rabbitMqContainer.GetConnectionString());
            ChannelPool = new RabbitMqChannelPool(_connManager);
        }

        public async Task DisposeAsync()
        {
            if (ChannelPool is not null) await ChannelPool.DisposeAsync();
            if (_connManager is not null) await _connManager.DisposeAsync();
            await _rabbitMqContainer.DisposeAsync();
        }
    }

    [CollectionDefinition("RabbitMqCollection")]
    public class RabbitMqCollection : ICollectionFixture<RabbitMqFixture>;
}
