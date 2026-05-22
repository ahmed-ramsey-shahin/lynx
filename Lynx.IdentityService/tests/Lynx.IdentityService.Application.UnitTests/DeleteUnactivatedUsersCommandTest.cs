using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Features.Identity.Commands.DeletedUnactivatedUsers;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class DeleteUnactivatedUsersCommandTest
    {
        private readonly Mock<ILogger<DeleteUnactivatedUsersCommandHandler>> _logger = new(MockBehavior.Loose);
        private DeleteUnactivatedUsersCommandHandler? _handler;

        private void CreateHandler(IUserRepository userRepository, TimeProvider timeProvider)
        {
            _handler = new(_logger.Object, userRepository, timeProvider);
        }

        [Fact]
        public async Task Handler_Should_DeleteAllUnactivatedUsersWhoPassedThe2HourActivationLimit()
        {
            // Arrange
            var timeProvider = new FakeTimeProvider();
            DateTimeOffset utcNow = new(2026, 2, 2, 10, 0, 0, TimeSpan.Zero);
            DateTimeOffset nowMinus2Hours = new(2026, 2, 2, 8, 0, 0, TimeSpan.Zero);
            timeProvider.SetUtcNow(utcNow);
            var userRepository = new UserRepositoryMockBuilder().WithSuccessfulDeleteUnactivatedUsers(nowMinus2Hours, 10);
            var request = new DeleteUnactivatedUsersCommand();
            CreateHandler(userRepository.Object, timeProvider);

            // Act
            await _handler!.Handle(request, default);

            // Assert
            userRepository.Mock.Verify(repo => repo.DeleteUnactivatedUsersAsync(
                nowMinus2Hours,
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }
    }
}
