using FluentAssertions;
using Lynx.RedirectionService.Application.Common.Behaviors;
using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests.Behaviors
{
    public class UnhandledExceptionBehaviorTests
    {
        private readonly Mock<ILogger<TestCommand>> _logger = new(MockBehavior.Loose);
        private readonly Mock<RequestHandlerDelegate<Result<TestResponse>>> _nextMock = new(MockBehavior.Strict);
        private readonly UnhandledExceptionBehavior<TestCommand, Result<TestResponse>> _behavior;

        public UnhandledExceptionBehaviorTests()
        {
            _behavior = new(_logger.Object);
        }

        [Fact]
        public async Task Handle_Should_CallNextAndReturnResult_WhenNextDoesntThrowExceptions()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedResponse);
            _nextMock.Verify(x => x(), Times.Once());
        }

        [Fact]
        public async Task Handle_ShouldLogAndThrowException_WhenNextThrowsException()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            var exception = new Exception("Test");
            _nextMock.Setup(x => x()).ThrowsAsync(exception);
            _logger.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            // Act
            // Assert
            var handleCall = () => _behavior.Handle(command, _nextMock.Object, default);
            await handleCall.Should().ThrowAsync<Exception>();
            _nextMock.Verify(x => x(), Times.Once());
            _logger.VerifyLogWithException(LogLevel.Error, null, exception, Times.AtLeastOnce());
        }
    }
}
