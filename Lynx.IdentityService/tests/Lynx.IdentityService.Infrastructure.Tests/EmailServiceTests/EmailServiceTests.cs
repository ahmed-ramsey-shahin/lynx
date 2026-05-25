using FluentAssertions;
using Lynx.IdentityService.Infrastructure.Configurations;
using Lynx.IdentityService.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Lynx.IdentityService.Infrastructure.Tests.EmailServiceTests
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_ShouldPostCorrectJsonPayload_ToBrevoApi()
        {
            // Arrange
            var fakeHandler = new FakeHttpMessageHandler();
            var fakeClient = new HttpClient(fakeHandler)
            {
                BaseAddress = new Uri("https://api.brevo.com/v3/smtp/email")
            };
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(fakeClient);
            var configOptions = Options.Create(new EmailServiceConfigurations
            {
                SenderEmail = "lynx@lynx.com",
                SenderName = "lynx"
            });
            var emailService = new EmailService(httpClientFactoryMock.Object, configOptions);

            // Act
            await emailService.SendEmailAsync(
                "target@lynx.com",
                "lynx_user",
                "SampleSubject",
                "SampleMessage",
                default
            );

            // Assert
            fakeHandler.CapturedRequest.Should().NotBeNull();
            fakeHandler.CapturedRequest!.Method.Should().Be(HttpMethod.Post);
            fakeHandler.CapturedRequest.RequestUri!.ToString().Should().Be("https://api.brevo.com/v3/smtp/email");
            fakeHandler.CapturedContent.Should().Contain("lynx_user");
            fakeHandler.CapturedContent.Should().Contain("target@lynx.com");
            fakeHandler.CapturedContent.Should().Contain("SampleSubject");
            fakeHandler.CapturedContent.Should().Contain("SampleMessage");
            fakeHandler.CapturedContent.Should().Contain("lynx@lynx.com");
            fakeHandler.CapturedContent.Should().Contain("lynx");
        }
    }
}
