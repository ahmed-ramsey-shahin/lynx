using Lynx.IdentityService.Application.Common.Services;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class PasswordHashingServiceMockBuilder
    {
        public Mock<IPasswordHashingService> Mock { get; } = new(MockBehavior.Strict);
        public IPasswordHashingService Object => Mock.Object;

        public PasswordHashingServiceMockBuilder WithSuccessfulHash(string @out, string? @in=null)
        {
            Mock.Setup(service => service.Hash(
                @in ?? It.IsAny<string>()
            )).Returns(@out);
            return this;
        }

        public PasswordHashingServiceMockBuilder WithSuccessfulVerify(string? password=null, string? hash=null)
        {
            Mock.Setup(service => service.Verify(
                password ?? It.IsAny<string>(),
                hash ?? It.IsAny<string>()
            )).Returns(true);
            return this;
        }

        public PasswordHashingServiceMockBuilder WithFailedVerify(string? password=null, string? hash=null)
        {
            Mock.Setup(service => service.Verify(
                password ?? It.IsAny<string>(),
                hash ?? It.IsAny<string>()
            )).Returns(false);
            return this;
        }
    }
}
