using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Contracts;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class EmailBackgroundQueueMockBuilder
    {
        public Mock<IEmailBackgroundQueue> Mock { get; } = new(MockBehavior.Strict);
        public IEmailBackgroundQueue Object => Mock.Object;

        public EmailBackgroundQueueMockBuilder WithSuccessfulQueueEmail()
        {
            Mock.Setup(service => service.QueueEmailAsync(
                It.IsAny<EmailJob>(),
                It.IsAny<CancellationToken>()
            )).Returns(ValueTask.CompletedTask);
            return this;
        }
    }
}
