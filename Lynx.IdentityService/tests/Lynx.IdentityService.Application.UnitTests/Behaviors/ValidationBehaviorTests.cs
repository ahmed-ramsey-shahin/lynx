using FluentAssertions;
using Lynx.IdentityService.Application.Common.Behaviors;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.Behaviors
{
    public class ValidationBehaviorTests
    {
        private readonly Mock<ILogger<ValidationBehavior<TestCommand, Result<TestResponse>>>> _logger = new(MockBehavior.Loose);
        private readonly Mock<RequestHandlerDelegate<Result<TestResponse>>> _nextMock = new(MockBehavior.Strict);
        private TestCommandValidator? _validator = new();
        private ValidationBehavior<TestCommand, Result<TestResponse>> _behavior;

        public ValidationBehaviorTests()
        {
            _behavior = new(_logger.Object, _validator);
        }

        [Fact]
        public async Task Handle_Should_CallNextOnly_WhenValidatorIsNull()
        {
            // Arrange
            var command = new TestCommand("Name", "IdempotencyKey");
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;
            _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);
            _validator = null;
            _behavior = new(_logger.Object, _validator);

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedResponse);
            _nextMock.Verify(x => x(), Times.Once());
        }

        [Fact]
        public async Task Handle_Should_ReturnCorrectResult_WhenValidationSucceeds()
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
        public async Task Handle_Should_ReturnValidationError_WhenValidationFails()
        {
            // Arrange
            var command = new TestCommand("  ", "IdempotencyKey");
            var expectedResponse = new TestResponse(3);
            var expectedResult = (Result<TestResponse>) expectedResponse;

            // Act
            var result = await _behavior.Handle(command, _nextMock.Object, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be("Name");
        }
    }
}
