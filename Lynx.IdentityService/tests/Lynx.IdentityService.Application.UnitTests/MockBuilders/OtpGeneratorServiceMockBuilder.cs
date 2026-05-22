using Lynx.IdentityService.Application.Common.Services;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class OtpGeneratorServiceMockBuilder
    {
        public Mock<IOTPGeneratorService> Mock { get; } = new(MockBehavior.Strict);
        public IOTPGeneratorService Object => Mock.Object;

        public OtpGeneratorServiceMockBuilder WithGenerateResetCode(string? result, int length=6)
        {
            Mock.Setup(service => service.GenerateResetCode(
                length
            )).Returns(result ?? string.Empty);
            return this;
        }

        public OtpGeneratorServiceMockBuilder WithGenerateUrlSafeToken(string? result)
        {
            Mock.Setup(service => service.GenerateUrlSafeToken(
                It.IsAny<int>()
            )).Returns(result ?? string.Empty);
            return this;
        }
    }
}
