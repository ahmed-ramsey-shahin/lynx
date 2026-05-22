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
                It.Is<string>(v => @in == null || @in== v)
            )).Returns(@out);
            return this;
        }

        public PasswordHashingServiceMockBuilder WithSuccessfulVerify(string? password=null, string? hash=null)
        {
            Mock.Setup(service => service.Verify(
                It.Is<string>(v => password == null || password == v),
                It.Is<string>(v => hash == null || hash == v)
            )).Returns(true);
            return this;
        }

        public PasswordHashingServiceMockBuilder WithFailedVerify(string? password=null, string? hash=null)
        {
            Mock.Setup(service => service.Verify(
                It.Is<string>(v => password == null || password == v),
                It.Is<string>(v => hash == null || hash == v)
            )).Returns(false);
            return this;
        }
    }
}
