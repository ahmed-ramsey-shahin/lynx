using System.Text.Json.Nodes;
using FluentAssertions;
using Lynx.IdentityService.Infrastructure.Services.RabbitMq;
using Lynx.IdentityService.Infrastructure.Tests.Fixtures;

namespace Lynx.IdentityService.Infrastructure.Tests
{
    [Collection("RabbitMqCollection")]
    public class MessagePublishingServiceTests(RabbitMqFixture fixture) : IAsyncLifetime
    {
        private readonly IRabbitMqChannelPool _channelPool = fixture.ChannelPool;
        private MessagePublishingService? _messageService;
        private readonly List<string> _testQueues = ["DeletedUsers"];

        public async Task InitializeAsync()
        {
            _messageService = new(_channelPool);
            var channel = await _channelPool.GetChannelAsync();

            try
            {
                foreach (var queue in _testQueues)
                {
                    await channel.QueueDeclareAsync(
                        queue: queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false
                    );
                }
            }
            finally
            {
                _channelPool.ReturnChannel(channel);
            }
        }


        [Fact]
        public async Task PublishAsync_Should_AddMessageToTheRabbitMq_WhenBodyIsGuid()
        {
            // Arrange
            var body = Guid.NewGuid();
            var queueName = _testQueues[0];

            // Act
            await _messageService!.PublishAsync(queueName, body, default);

            // Assert
            var channel = await _channelPool.GetChannelAsync();

            try
            {
                var result = await channel.BasicGetAsync(queueName, true);
                result.Should().NotBeNull();
                result.Body.Should().BeEquivalentTo(body.ToByteArray());
            }
            finally
            {
                _channelPool.ReturnChannel(channel);
            }
        }

        [Fact]
        public async Task PublishAsync_Should_AddMessageToTheRabbitMq_WhenBodyIsObject()
        {
            // Arrange
            var body = new
            {
                Id = Guid.NewGuid(),
                Name = "TestUser"
            };
            var queueName = _testQueues[0];

            // Act
            await _messageService!.PublishAsync(queueName, body, default);

            // Assert
            var channel = await _channelPool.GetChannelAsync();

            try
            {
                var result = await channel.BasicGetAsync(queueName, true);
                result.Should().NotBeNull();
                var jsonNode = JsonNode.Parse(result.Body.ToArray());
                var receivedId = jsonNode!["Id"]!.GetValue<Guid>();
                var receivedName = jsonNode!["Name"]!.GetValue<string>();
                receivedId.Should().Be(body.Id);
                receivedName.Should().Be(body.Name);
            }
            finally
            {
                _channelPool.ReturnChannel(channel);
            }
        }

        public async Task DisposeAsync()
        {
            var channel = await _channelPool.GetChannelAsync();

            try
            {
                foreach (var queue in _testQueues)
                {
                    await channel.QueuePurgeAsync(queue);
                }
            }
            finally
            {
                _channelPool.ReturnChannel(channel);
            }
        }
    }
}
