using Lynx.RedirectionService.Application.Common.Services;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests.MockBuilders
{
    public class MessagePublishingServiceMockBuilder
    {
        public Mock<IMessagePublishingService> Mock { get; } = new(MockBehavior.Strict);
        public IMessagePublishingService Object => Mock.Object;

        public MessagePublishingServiceMockBuilder WithSuccessfulPublish<T>(string? queue=null, T? body=default)
        {
            Mock.Setup(service => service.PublishAsync(
                It.Is<string>(v => queue == null || queue == v),
                It.Is<T>(v => body == null || body.Equals(v)),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            return this;
        }
    }
}
