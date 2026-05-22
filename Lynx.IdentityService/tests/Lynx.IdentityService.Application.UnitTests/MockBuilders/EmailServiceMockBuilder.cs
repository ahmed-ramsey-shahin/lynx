using Lynx.IdentityService.Application.Common.Services;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class EmailServiceMockBuilder
    {
        public Mock<IEmailService> Mock { get; } = new(MockBehavior.Strict);
        public IEmailService Object => Mock.Object;

        public EmailServiceMockBuilder WithSuccessfulSendEmail()
        {
            Mock.Setup(service => service.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            return this;
        }
    }
}
