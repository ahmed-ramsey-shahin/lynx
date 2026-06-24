using FluentAssertions;
using Lynx.RedirectionService.Application.Common.Behaviors;
using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests.Behaviors
{
    public class CachingBehaviorTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CachingBehavior<TestCommand, Result<TestResponse>>>> _loggerMock = new(MockBehavior.Loose);
        private readonly Mock<RequestHandlerDelegate<Result<TestResponse>>> _nextMock = new(MockBehavior.Strict);
        private readonly CachingBehavior<TestCommand, Result<TestResponse>> _behavior;

        public CachingBehaviorTests()
        {
            _behavior = new(
                _cacheServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_CallNextAndSaveResultToCache_WhenThereAreNoCache()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);
            _cacheServiceMock.Setup(service => service.GetAsync<Result<TestResponse>>(
                command.CacheKey,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync((Result<TestResponse>?) null);
            _cacheServiceMock.Setup(service => service.SetAsync(
                command.CacheKey,
                expectedResult,
                command.Expiration
            )).Returns(Task.CompletedTask);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _nextMock.Verify(x => x(), Times.Once());
            _cacheServiceMock.Verify(service => service.GetAsync<Result<TestResponse>>(
                command.CacheKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.SetAsync(
                command.CacheKey,
                expectedResult,
                command.Expiration
            ), Times.Once());
            result.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task Handle_Should_CallNextAndSaveNothingToCache_WhenCommandIsNotICachedQuery()
        {
            // Arrange
            Mock<ILogger<CachingBehavior<RegularCommand, Result<TestResponse>>>> _loggerMock = new(MockBehavior.Loose);
            CachingBehavior<RegularCommand, Result<TestResponse>> _behavior = new(
                _cacheServiceMock.Object,
                _loggerMock.Object
            );
            var command = new RegularCommand();
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _nextMock.Verify(x => x(), Times.Once());
            result.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task Handle_Should_ReturnResultFromCacheIfItsFound()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _cacheServiceMock.Setup(service => service.GetAsync<Result<TestResponse>>(
                command.CacheKey,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(expectedResult);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _cacheServiceMock.Verify(service => service.GetAsync<Result<TestResponse>>(
                command.CacheKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            result.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task Handle_Should_SaveNothingToCache_WhenNextReturnsError()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) Error.Failure("code", "description");
            _cacheServiceMock.Setup(service => service.GetAsync<Result<TestResponse>>(
                command.CacheKey,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync((Result<TestResponse>?) null);
            _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be("code");
            _cacheServiceMock.Verify(service => service.GetAsync<Result<TestResponse>>(
                command.CacheKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _nextMock.Verify(x => x(), Times.Once());
        }
    }
}
