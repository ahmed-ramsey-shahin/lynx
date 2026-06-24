using FluentAssertions;
using Lynx.RedirectionService.Application.Common.Behaviors;
using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests.Behaviors
{
    public class IdempotencyBehaviorTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock = new(MockBehavior.Strict);
        private readonly Mock<ILogger<IdempotencyBehavior<TestCommand, Result<TestResponse>>>> _logger = new(MockBehavior.Loose);
        private readonly Mock<RequestHandlerDelegate<Result<TestResponse>>> _nextMock = new(MockBehavior.Strict);
        private readonly IdempotencyBehavior<TestCommand, Result<TestResponse>> _behavior;

        public IdempotencyBehaviorTests()
        {
            _behavior = new(_cacheServiceMock.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_Should_CallNextAndSaveToCache_WhenKeyAndLockAreNotFound()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var lockKey = $"{command.IdempotencyKey}_lock";
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _cacheServiceMock.Setup(service => service.TrySetAsync(
                lockKey,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(true);
            _cacheServiceMock.Setup(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync((Result<TestResponse>?) null);
            _cacheServiceMock.Setup(service => service.SetAsync(
                command.IdempotencyKey,
                expectedResult,
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            _cacheServiceMock.Setup(service => service.RemoveAsync(
                lockKey,
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedResponse);
            _nextMock.Verify(x => x(), Times.Once());
            _cacheServiceMock.Verify(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.RemoveAsync(
                lockKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.SetAsync(
                command.IdempotencyKey,
                expectedResult,
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.TrySetAsync(
                lockKey,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }

        [Fact]
        public async Task Handle_Should_ReturnResultFromCache_WhenFoundInCache()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var lockKey = $"{command.IdempotencyKey}_lock";
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _cacheServiceMock.Setup(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(expectedResult);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedResponse);
            _cacheServiceMock.Verify(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }

        [Fact]
        public async Task Handle_Should_ReturnConflictError_WhenTrySetFails()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var lockKey = $"{command.IdempotencyKey}_lock";
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _cacheServiceMock.Setup(service => service.TrySetAsync(
                lockKey,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(false);
            _cacheServiceMock.Setup(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync((Result<TestResponse>?) null);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be("Idempotency.Conflict");
            _cacheServiceMock.Verify(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.TrySetAsync(
                lockKey,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }

        [Fact]
        public async Task Handle_Should_SaveNothingToCacheAndReturnError_WhenNextReturnsError()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var lockKey = $"{command.IdempotencyKey}_lock";
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) Error.Failure("code", "description");
            _cacheServiceMock.Setup(service => service.TrySetAsync(
                lockKey,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(true);
            _cacheServiceMock.Setup(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync((Result<TestResponse>?) null);
            _cacheServiceMock.Setup(service => service.RemoveAsync(
                lockKey,
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be("code");
            _nextMock.Verify(x => x(), Times.Once());
            _cacheServiceMock.Verify(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.TrySetAsync(
                lockKey,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.RemoveAsync(
                lockKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }

        [Fact]
        public async Task Handle_Should_CallNextAndSaveNothingToCache_WhenCommandIsNotAnIdempotentCommand()
        {
            // Arrange
            Mock<ILogger<IdempotencyBehavior<RegularCommand, Result<TestResponse>>>> _logger = new(MockBehavior.Loose);
            IdempotencyBehavior<RegularCommand, Result<TestResponse>> _behavior = new(
                _cacheServiceMock.Object,
                _logger.Object
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
        public async Task Handle_Should_ReleaseLock_WhenNextThrowsException()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var lockKey = $"{command.IdempotencyKey}_lock";
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _cacheServiceMock.Setup(service => service.TrySetAsync(
                lockKey,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(true);
            _cacheServiceMock.Setup(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync((Result<TestResponse>?) null);
            _cacheServiceMock.Setup(service => service.RemoveAsync(
                lockKey,
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            _nextMock.Setup(x => x()).Throws(new Exception());

            // Act
            // Assert
            var callHandle = () => _behavior.Handle(command, _nextMock.Object, default);
            await callHandle.Should().ThrowAsync<Exception>();
            _nextMock.Verify(x => x(), Times.Once());
            _cacheServiceMock.Verify(service => service.GetAsync<Result<TestResponse>>(
                command.IdempotencyKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.RemoveAsync(
                lockKey,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _cacheServiceMock.Verify(service => service.TrySetAsync(
                lockKey,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }
    }
}
