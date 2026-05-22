using FluentAssertions;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUsername;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class ChangeUsernameCommandTests
    {
        private readonly Mock<ILogger<ChangeUsernameCommandHandler>> _logger = new(MockBehavior.Loose);
        private ChangeUsernameCommandHandler? _handler;

        private void CreateHandler(
            IUserRepository userRepository,
            IPasswordHashingService hashingService,
            ICacheService cacheService,
            IUserService userService
        )
        {
            _handler = new ChangeUsernameCommandHandler(
                _logger.Object,
                userRepository,
                hashingService,
                cacheService,
                userService
            );
        }

        [Fact]
        public async Task Handler_Should_ReturnUpdated_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string newUsername = "very_creative_and_cool_new_username";
            const string oldUsername = "very_boring_old_username";
            const string password = "MyVeryStrongOldPassword@Lynx123";
            const string passwordHash = "MyVeryStrongOldPassword@Lynx123_AfterHashing";
            string cacheKey = $"users:{oldUsername}";
            var request = new ChangeUsernameCommand()
            {
                UserId = userId,
                Password = password,
                NewUsername = newUsername
            };
            var user = User.Create(userId, "user@lynx.com", oldUsername, passwordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var hashing = new PasswordHashingServiceMockBuilder()
                .WithSuccessfulVerify(password, passwordHash);
            var userRepo = new UserRepositoryMockBuilder()
                .WithUserById(userId, user)
                .WithSuccessfulDatabaseUpdate()
                .WithUniqueUsername(newUsername);
            var cacheService = new CacheServiceMockBuilder()
                .WithSuccessfulRemove(cacheKey);
            var userService = new UserServiceMockBuilder()
                .WithId(userId);
            CreateHandler(userRepo.Object, hashing.Object, cacheService.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            hashing.Mock.Verify(service => service.Verify(password, passwordHash), Times.Once());
            userRepo.Mock.Verify(repo => repo.UpdateAsync(It.Is<User>(user =>
                user.Id == userId &&
                user.Username == newUsername
            ), It.IsAny<CancellationToken>()), Times.Once());
            userService.Mock.Verify(service => service.UserId, Times.AtLeastOnce());
            userRepo.Mock.Verify(repo => repo.IsUsernameUniqueAsync(newUsername, It.IsAny<CancellationToken>()), Times.Once());
            cacheService.Mock.Verify(service => service.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
            user.Username.Should().Be(newUsername);
        }
    }
}
