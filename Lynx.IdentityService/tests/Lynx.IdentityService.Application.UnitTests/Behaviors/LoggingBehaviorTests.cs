using FluentAssertions;
using Lynx.IdentityService.Application.Common.Behaviors;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.Behaviors
{
    public class LoggingBehaviorTests
    {
        private readonly Mock<ILogger<LoggingBehavior<TestCommand, Result<TestResponse>>>> _loggerMock = new(MockBehavior.Loose);
        private readonly Mock<IUserService> _userServiceMock = new(MockBehavior.Strict);
        private readonly Mock<RequestHandlerDelegate<Result<TestResponse>>> _nextMock = new(MockBehavior.Strict);
        private readonly LoggingBehavior<TestCommand, Result<TestResponse>> _behavior;

        public LoggingBehaviorTests()
        {
            _behavior = new(_loggerMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task Handle_Should_CallNextAndLog()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);
            _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _userServiceMock.Setup(service => service.UserId).Returns(Guid.NewGuid());

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.Is<Exception>(_ => true),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.AtLeastOnce());
            _nextMock.Verify(x => x(), Times.Once());
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedResponse);
            _userServiceMock.Verify(service => service.UserId, Times.AtLeastOnce());
        }
    }
}
