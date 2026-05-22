using Lynx.IdentityService.Application.Common.Services;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class CacheServiceMockBuilder
    {
        public Mock<ICacheService> Mock { get; } = new(MockBehavior.Strict);
        public ICacheService Object => Mock.Object;

        public CacheServiceMockBuilder WithSuccessfulSet<T>()
        {
            Mock.Setup(service => service.SetAsync(
                It.IsAny<string>(),
                It.IsAny<T>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            return this;
        }

        public CacheServiceMockBuilder WithSuccessfulGet<T>(string? key=null, T? returnValue=default)
        {
            Mock.Setup(service => service.GetAsync<T>(
                key ?? It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(returnValue);
            return this;
        }

        public CacheServiceMockBuilder WithSuccessfulRemove(string? key=null)
        {
            Mock.Setup(service => service.RemoveAsync(
                key ?? It.IsAny<string>(),
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
