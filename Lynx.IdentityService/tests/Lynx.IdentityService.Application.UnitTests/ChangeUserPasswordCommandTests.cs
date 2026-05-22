using FluentAssertions;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUserPassword;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class ChangeUserPasswordCommandTests
    {
        private readonly Mock<ILogger<ChangeUserPasswordCommandHandler>> _logger = new(MockBehavior.Loose);
        private ChangeUserPasswordCommandHandler? _handler;

        private void CreateHandler(IUserRepository userRepo, IPasswordHashingService hashingService, IUserService userService)
        {
            _handler = new(_logger.Object, userRepo, hashingService, userService);
        }

        [Fact]
        public async Task Handler_Should_ReturnUpdated_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string oldPassword = "MyVeryStrongOldPassword@Lynx123";
            const string newPassword = "MyVeryStrongNewPassword@Lynx123";
            const string oldPasswordHash = "MyVeryStrongOldPassword@Lynx123_AfterHashing";
            const string newPasswordHash = "MyVeryStrongNewPassword@Lynx123_AfterHashing";
            var request = new ChangeUserPasswordCommand()
            {
                NewPassword = newPassword,
                OldPassword = oldPassword,
                UserId = userId
            };
            var user = User.Create(userId, "user@lynx.com", "awesome_username", oldPasswordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var hashing = new PasswordHashingServiceMockBuilder()
                .WithSuccessfulHash(newPasswordHash, newPassword)
                .WithSuccessfulVerify(oldPassword, oldPasswordHash);
            var userRepo = new UserRepositoryMockBuilder()
                .WithUserById(userId, user)
                .WithSuccessfulDatabaseUpdate();
            var userService = new UserServiceMockBuilder()
                .WithId(userId);
            CreateHandler(userRepo.Object, hashing.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            hashing.Mock.Verify(service => service.Verify(oldPassword, oldPasswordHash), Times.Once());
            hashing.Mock.Verify(service => service.Hash(newPassword), Times.Once());
            userRepo.Mock.Verify(repo => repo.UpdateAsync(It.Is<User>(user =>
                user.Id == userId &&
                user.Password == newPasswordHash
            ), It.IsAny<CancellationToken>()), Times.Once());
            userService.Mock.Verify(service => service.UserId, Times.AtLeastOnce());
            user.Password.Should().Be(newPasswordHash);
        }

        [Fact]
        public async Task Handler_Should_ReturnInvalidOldPassword_WhenOldPasswordIsWrong()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string oldPassword = "WrongOldPassword";
            const string newPassword = "MyVeryStrongNewPassword@Lynx123";
            const string oldPasswordHash = "MyVeryStrongOldPassword@Lynx123_AfterHashing";
            var request = new ChangeUserPasswordCommand()
            {
                NewPassword = newPassword,
                OldPassword = oldPassword,
                UserId = userId
            };
            var user = User.Create(userId, "user@lynx.com", "awesome_username", oldPasswordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var hashing = new PasswordHashingServiceMockBuilder()
                .WithFailedVerify(oldPassword, oldPasswordHash);
            var userRepo = new UserRepositoryMockBuilder()
                .WithUserById(userId, user);
            var userService = new UserServiceMockBuilder()
                .WithId(userId);
            CreateHandler(userRepo.Object, hashing.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.InvalidOldPassword.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            hashing.Mock.Verify(service => service.Verify(oldPassword, oldPasswordHash), Times.Once());
            user.Password.Should().Be(oldPasswordHash);
            userService.Mock.Verify(service => service.UserId, Times.AtLeastOnce());
        }

        [Fact]
        public async Task Handler_Should_ReturnNotFound_WhenUserIdIsWrong()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string oldPassword = "MyVeryStrongOldPassword@Lynx123";
            const string newPassword = "MyVeryStrongNewPassword@Lynx123";
            const string newPasswordHash = "MyVeryStrongNewPassword@Lynx123_AfterHashing";
            var request = new ChangeUserPasswordCommand()
            {
                NewPassword = newPassword,
                OldPassword = oldPassword,
                UserId = userId
            };
            var hashing = new PasswordHashingServiceMockBuilder().WithSuccessfulHash(newPasswordHash, newPassword);
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, null);
            var userService = new UserServiceMockBuilder().WithId(userId);
            CreateHandler(userRepo.Object, hashing.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UserNotFound.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnUserNotOwned_WhenUserIdIsDifferentThanAuthenticatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string oldPassword = "MyVeryStrongOldPassword@Lynx123";
            const string newPassword = "MyVeryStrongNewPassword@Lynx123";
            const string oldPasswordHash = "MyVeryStrongOldPassword@Lynx123_AfterHashing";
            const string newPasswordHash = "MyVeryStrongNewPassword@Lynx123_AfterHashing";
            var request = new ChangeUserPasswordCommand()
            {
                NewPassword = newPassword,
                OldPassword = oldPassword,
                UserId = userId
            };
            var user = User.Create(userId, "user@lynx.com", "awesome_username", oldPasswordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var hashing = new PasswordHashingServiceMockBuilder()
                .WithSuccessfulHash(newPasswordHash, newPassword)
                .WithSuccessfulVerify(oldPassword, oldPasswordHash);
            var userRepo = new UserRepositoryMockBuilder()
                .WithUserById(userId, user);
            var userService = new UserServiceMockBuilder()
                .WithId(Guid.NewGuid());
            CreateHandler(userRepo.Object, hashing.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UserNotOwned.Code);
            userService.Mock.Verify(service => service.UserId, Times.AtLeastOnce());
            user.Password.Should().Be(oldPasswordHash);
        }
    }
}
