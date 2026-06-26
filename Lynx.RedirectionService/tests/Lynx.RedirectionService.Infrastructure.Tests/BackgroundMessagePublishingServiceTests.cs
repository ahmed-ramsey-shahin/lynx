using System.Text.Json;
using Lynx.RedirectionService.Infrastructure.Services.RabbitMq;
using Moq;

namespace Lynx.RedirectionService.Infrastructure.Tests
{
    public class BackgroundMessagePublishingServiceTests
    {
        [Fact]
        public async Task PublishAsync_Should_PublishTheCorrectMessageToChannel()
        {
            // Arrange
            var channelMock = new Mock<IMessageChannel>();
            var sut = new BackgroundMessagePublishingService(channelMock.Object);
            const string queueName = "queue-name";
            var body = new
            {
                id = 1,
                key = "hello"
            };
            string expectedJson = JsonSerializer.Serialize(body);

            // Act
            await sut.PublishAsync(queueName, body, default);

            // Assert
            channelMock.Verify(channel => channel.QueueMessageAsync(
                It.Is<QueuedMessage>(msg =>
                    msg.QueueName == queueName &&
                    msg.SerializedBody == expectedJson
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }
    }
}
