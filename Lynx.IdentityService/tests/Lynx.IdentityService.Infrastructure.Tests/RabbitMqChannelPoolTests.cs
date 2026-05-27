using System.Collections.Concurrent;
using FluentAssertions;
using Lynx.IdentityService.Infrastructure.Services.RabbitMq;
using Moq;
using RabbitMQ.Client;

namespace Lynx.IdentityService.Infrastructure.Tests
{
    public class RabbitMqChannelPoolTests
    {
        private readonly Mock<IConnection> _connectionMock = new(MockBehavior.Loose);
        private readonly Mock<IRabbitMqConnectionManager> _connectionManagerMock = new(MockBehavior.Loose);
        private readonly Mock<IChannel> _channelMock = new(MockBehavior.Loose);
        private readonly RabbitMqChannelPool _channelPool;
        private readonly int _maxPoolSize = 10;

        public RabbitMqChannelPoolTests()
        {
            _channelPool = new(_connectionManagerMock.Object, _maxPoolSize);
            _connectionMock.Setup(conn => conn.CreateChannelAsync())
                .ReturnsAsync(_channelMock.Object);
            _connectionManagerMock.Setup(manager => manager.GetConnectionAsync())
                .ReturnsAsync(_connectionMock.Object);
        }

        [Fact]
        public async Task ChannelPool_Should_SurviveMassiveConcurrentRequests_WithoutCraching()
        {
            // Arrange
            const int concurrentRequests = 50;
            var tasks = new List<Task>();
            var acquiredChannels = new ConcurrentBag<IChannel>();
            _channelMock.Setup(c => c.IsOpen).Returns(true);

            // Act
            for (int i = 0; i < concurrentRequests; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var channel = await _channelPool.GetChannelAsync();
                    acquiredChannels.Add(channel);
                    await Task.Delay(Random.Shared.Next(5, 20));
                    _channelPool.ReturnChannel(channel);
                }));
            }
            var act = async() => await Task.WhenAll(tasks);

            // Assert
            await act.Should().NotThrowAsync();
            acquiredChannels.Should().HaveCount(concurrentRequests);
            _connectionMock.Verify(c => c.CreateChannelAsync(
                It.IsAny<CreateChannelOptions?>(),
                It.IsAny<CancellationToken>()
            ), Times.AtMost(_maxPoolSize));
        }
    }
}
