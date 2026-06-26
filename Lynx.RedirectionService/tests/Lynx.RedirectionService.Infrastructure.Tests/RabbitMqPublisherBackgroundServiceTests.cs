using System.Runtime.CompilerServices;
using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Infrastructure.BackgroundServices;
using Lynx.RedirectionService.Infrastructure.Services.RabbitMq;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.RedirectionService.Infrastructure.Tests
{
    public class RabbitMqPublisherBackgroundServiceTests
    {
        private readonly Mock<IMessageChannel> _channelMock;
        private readonly Mock<IMessagePublishingService> _publishingServiceMock;
        private readonly Mock<ILogger<RabbitMqPublisherBackgroundService>> _loggerMock;

        public RabbitMqPublisherBackgroundServiceTests()
        {
            _channelMock = new();
            _publishingServiceMock = new();
            _loggerMock = new();
        }

        [Fact]
        public async Task ExecuteAsync_Should_PublishAllMessagesInTheChannel()
        {
            // Arrange
            var messages = new[]
            {
                new QueuedMessage("queue-1", "body-1"),
                new QueuedMessage("queue-2", "body-2")
            };
            _channelMock
                .Setup(channel => channel.ReadAllAsync(It.IsAny<CancellationToken>()))
                .Returns(GetMessagesAsync(messages, default));
            var sut = new RabbitMqPublisherBackgroundService(_channelMock.Object, _loggerMock.Object, _publishingServiceMock.Object);

            // Act
            await sut.StartAsync(default);

            if (sut.ExecuteTask != null)
            {
                await sut.ExecuteTask;
            }

            _publishingServiceMock.Verify(p => p.PublishAsync("queue-1", "body-1", It.IsAny<CancellationToken>()), Times.Once());
            _publishingServiceMock.Verify(p => p.PublishAsync("queue-2", "body-2", It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCatchException_When_PublishFails()
        {
            // Arrange
            var messages = new[]
            {
                new QueuedMessage("queue-1", "body-1"),
                new QueuedMessage("queue-2", "body-2")
            };
            _channelMock
                .Setup(channel => channel.ReadAllAsync(It.IsAny<CancellationToken>()))
                .Returns(GetMessagesAsync(messages, default));
            _publishingServiceMock
                .Setup(p => p.PublishAsync("queue-1", "body-1", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Network failure"));
            var sut = new RabbitMqPublisherBackgroundService(_channelMock.Object, _loggerMock.Object, _publishingServiceMock.Object);

            // Act
            await sut.StartAsync(default);

            if (sut.ExecuteTask != null)
            {
                await sut.ExecuteTask;
            }

            // Assert
            _publishingServiceMock.Verify(p => p.PublishAsync("queue-1", "body-1", It.IsAny<CancellationToken>()), Times.Once());
            _publishingServiceMock.Verify(p => p.PublishAsync("queue-2", "body-2", It.IsAny<CancellationToken>()), Times.Once());
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, _) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)
                ),
                Times.Once
            );
        }

        private static async IAsyncEnumerable<QueuedMessage> GetMessagesAsync(
            IEnumerable<QueuedMessage> messages,
            [EnumeratorCancellation] CancellationToken cancellationToken=default
        )
        {
            foreach (var message in messages)
            {
                await Task.Yield();

                if (cancellationToken.IsCancellationRequested)
                    yield break;

                yield return message;
            }
        }
    }
}
