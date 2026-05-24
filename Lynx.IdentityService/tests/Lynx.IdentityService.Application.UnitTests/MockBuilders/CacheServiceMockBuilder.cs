using Lynx.IdentityService.Application.Common.Services;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class CacheServiceMockBuilder
    {
        public Mock<ICacheService> Mock { get; } = new(MockBehavior.Strict);
        public ICacheService Object => Mock.Object;

        public CacheServiceMockBuilder WithSuccessfulSet<T>(string? key=null, string? value=null, TimeSpan? expiresOn=null)
        {
            Mock.Setup(service => service.SetAsync(
                It.Is<string>(v => key == null || key == v),
                It.Is<T>(v => value == null || value.Equals(v)),
                It.Is<TimeSpan?>(v => expiresOn == null || expiresOn == v),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            return this;
        }

        public CacheServiceMockBuilder WithSuccessfulGet<T>(string? key=null, T? returnValue=default)
        {
            Mock.Setup(service => service.GetAsync<T>(
                It.Is<string>(v => key == null || key == v),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(returnValue);
            return this;
        }

        public CacheServiceMockBuilder WithSuccessfulRemove(string? key=null)
        {
            Mock.Setup(service => service.RemoveAsync(
                It.Is<string>(v => key == null || key == v),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            return this;
        }

        public CacheServiceMockBuilder WithSuccessfulTrySet<T>()
        {
            Mock.Setup(service => service.TrySetAsync(
                It.IsAny<string>(),
                It.IsAny<T>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(true);
            return this;
        }

        public CacheServiceMockBuilder WithFailedTrySet<T>()
        {
            Mock.Setup(service => service.TrySetAsync(
                It.IsAny<string>(),
                It.IsAny<T>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(false);
            return this;
        }
    }
}
