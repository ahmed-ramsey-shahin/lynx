using FluentAssertions;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Commands.DeleteUser;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class DeleteUserCommandTests
    {
        private readonly Mock<ILogger<DeleteUserCommandHandler>> _logger = new(MockBehavior.Loose);
        private readonly FakeTimeProvider _timeProvider = new();
        private DeleteUserCommandHandler? _handler;

        private void CreateHandler(
            IPasswordHashingService hashingService,
            IUserRepository userRepo,
            IMessagePublishingService publishingService,
            ICacheService cacheService,
            IUserService userService
        )
        {
            var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
            _timeProvider.SetUtcNow(frozenTime);
            _handler = new DeleteUserCommandHandler(
                hashingService,
                _logger.Object,
                userRepo,
                publishingService,
                cacheService,
                _timeProvider,
                userService
            );
        }

        [Fact]
        public async Task Handler_Should_ReturnDeleted_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string password = "VeryStrongPassword";
            const string passwordHash = "VeryStrongPassword_AfterHashing";
            const string username = "lynx_user";
            string cacheKey = $"users:{username}";
            const bool hasConfirmed = true;
            var request = new DeleteUserCommand()
            {
                Password = password,
                UserId = userId,
                HasConfirmed = hasConfirmed
            };
            var user = User.Create(userId, "user@lynx.com", username, passwordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var hashingService = new PasswordHashingServiceMockBuilder().WithSuccessfulVerify(password, passwordHash);
            var userRepo = new UserRepositoryMockBuilder()
                .WithUserById(userId, user)
                .WithSuccessfulDatabaseUpdate();
            var messagePublisher = new MessagePublishingServiceMockBuilder().WithSuccessfulPublish(null, userId);
            var cacheService = new CacheServiceMockBuilder().WithSuccessfulRemove(cacheKey);
            var userService = new UserServiceMockBuilder().WithId(userId);
            CreateHandler(hashingService.Object, userRepo.Object, messagePublisher.Object, cacheService.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.IsDeleted.Should().BeTrue();
            hashingService.Mock.Verify(service => service.Verify(password, passwordHash), Times.Once());
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            userRepo.Mock.Verify(repo => repo.UpdateAsync(
                It.Is<User>(user =>
                    user.Id == userId &&
                    user.IsDeleted &&
                    user.DeletedAt == _timeProvider.GetUtcNow()
                ), It.IsAny<CancellationToken>()
            ), Times.Once());
            messagePublisher.Mock.Verify(publisher => publisher.PublishAsync(It.IsAny<string>(), userId), Times.Once());
            userService.Mock.Verify(service => service.UserId, Times.AtLeastOnce());
            cacheService.Mock.Verify(service => service.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnUserNotFound_WhenUserIdIsWrong()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string password = "VeryStrongPassword";
            const string username = "lynx_user";
            string cacheKey = $"users:{username}";
            const bool hasConfirmed = true;
            var request = new DeleteUserCommand()
            {
                Password = password,
                UserId = userId,
                HasConfirmed = hasConfirmed
            };
            var hashingService = new PasswordHashingServiceMockBuilder();
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, null);
            var messagePublisher = new MessagePublishingServiceMockBuilder();
            var cacheService = new CacheServiceMockBuilder();
            var userService = new UserServiceMockBuilder().WithId(userId);
            CreateHandler(hashingService.Object, userRepo.Object, messagePublisher.Object, cacheService.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UserNotFound.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnInvalidOldPassword_WhenOldPasswordIsWrong()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string password = "VeryStrongWrongPassword";
            const string passwordHash = "VeryStrongPassword_AfterHashing";
            const string username = "lynx_user";
            string cacheKey = $"users:{username}";
            const bool hasConfirmed = true;
            var request = new DeleteUserCommand()
            {
                Password = password,
                UserId = userId,
                HasConfirmed = hasConfirmed
            };
            var user = User.Create(userId, "user@lynx.com", username, passwordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var hashingService = new PasswordHashingServiceMockBuilder().WithFailedVerify(password, passwordHash);
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, user);
            var messagePublisher = new MessagePublishingServiceMockBuilder();
            var cacheService = new CacheServiceMockBuilder();
            var userService = new UserServiceMockBuilder().WithId(userId);
            CreateHandler(hashingService.Object, userRepo.Object, messagePublisher.Object, cacheService.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.InvalidOldPassword.Code);
            hashingService.Mock.Verify(service => service.Verify(password, passwordHash), Times.Once());
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            user.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task Handler_Should_ReturnUserNotOwned_WhenUserIdIsDifferentThanAuthenticatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string password = "VeryStrongPassword";
            const string passwordHash = "VeryStrongPassword_AfterHashing";
            const string username = "lynx_user";
            string cacheKey = $"users:{username}";
            const bool hasConfirmed = true;
            var request = new DeleteUserCommand()
            {
                Password = password,
                UserId = userId,
                HasConfirmed = hasConfirmed
            };
            var user = User.Create(userId, "user@lynx.com", username, passwordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var hashingService = new PasswordHashingServiceMockBuilder().WithSuccessfulVerify(password, passwordHash);
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, user);
            var messagePublisher = new MessagePublishingServiceMockBuilder();
            var cacheService = new CacheServiceMockBuilder();
            var userService = new UserServiceMockBuilder().WithId(Guid.NewGuid());
            CreateHandler(hashingService.Object, userRepo.Object, messagePublisher.Object, cacheService.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UserNotOwned.Code);
            user.IsDeleted.Should().BeFalse();
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            userService.Mock.Verify(service => service.UserId, Times.AtLeastOnce());
        }

        [Fact]
        public async Task Handler_Should_ReturnDeletionNotConfirmed_WhenUserDidntConfirm()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string password = "VeryStrongPassword";
            const string passwordHash = "VeryStrongPassword_AfterHashing";
            const string username = "lynx_user";
            string cacheKey = $"users:{username}";
            const bool hasConfirmed = false;
            var request = new DeleteUserCommand()
            {
                Password = password,
                UserId = userId,
                HasConfirmed = hasConfirmed
            };
            var user = User.Create(userId, "user@lynx.com", username, passwordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var hashingService = new PasswordHashingServiceMockBuilder().WithSuccessfulVerify(password, passwordHash);
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, user);
            var messagePublisher = new MessagePublishingServiceMockBuilder();
            var cacheService = new CacheServiceMockBuilder();
            var userService = new UserServiceMockBuilder().WithId(userId);
            CreateHandler(hashingService.Object, userRepo.Object, messagePublisher.Object, cacheService.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.DeletionNotConfirmed.Code);
            user.IsDeleted.Should().BeFalse();
        }
    }
}
