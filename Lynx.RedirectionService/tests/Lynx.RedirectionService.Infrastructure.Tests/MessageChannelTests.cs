using FluentAssertions;
using Lynx.RedirectionService.Infrastructure.Services.RabbitMq;

namespace Lynx.RedirectionService.Infrastructure.Tests
{
    public class MessageChannelTests
    {
        [Fact]
        public async Task QueueMessageAsync_Should_MakeMessageAvailableForReading()
        {
            // Arrange
            var sut = new MessageChannel();
            var message = new QueuedMessage("test-queue", "hello, world");

            // Act
            await sut.QueueMessageAsync(message, default);

            // Assert
            var asyncEnumerator = sut.ReadAllAsync(default).GetAsyncEnumerator(default);
            var hasMessage = await asyncEnumerator.MoveNextAsync();
            hasMessage.Should().BeTrue();
            asyncEnumerator.Current.QueueName.Should().Be(message.QueueName);
            asyncEnumerator.Current.SerializedBody.Should().Be(message.SerializedBody);
        }
    }
}
