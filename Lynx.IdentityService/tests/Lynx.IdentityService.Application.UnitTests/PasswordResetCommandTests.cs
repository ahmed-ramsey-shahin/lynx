using FluentAssertions;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Commands.PasswordReset;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class PasswordResetCommandTests
    {
        private readonly Mock<ILogger<PasswordResetCommandHandler>> _logger = new(MockBehavior.Loose);
        private PasswordResetCommandHandler? _handler;

        private void CreateHandler(
            IPasswordHashingService hashingService,
            IUserRepository userRepo,
            ICacheService cacheService
        )
        {
            _handler = new(hashingService, userRepo, cacheService, _logger.Object);
        }

        [Fact]
        public async Task Handler_Should_ReturnUpdated_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string username = "lynx_user";
            const string email = "user@lynx.com";
            const string newPassword = "NewPassword@13";
            const string newPasswordHash = "NewPassword@13Hashed";
            const string oldPasswordHash = "VeryStrongPasswordName";
            const string otp = "132465";
            string cacheKey = $"reset_password_otps:{userId}";
            var user = User.Create(userId, email, username, oldPasswordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var request = new PasswordResetCommand()
            {
                Code = otp,
                Email = email,
                NewPassword = newPassword
            };
            var hashingService = new PasswordHashingServiceMockBuilder().WithSuccessfulHash(newPasswordHash, newPassword);
            var cacheService = new CacheServiceMockBuilder()
                .WithSuccessfulGet(cacheKey, otp)
                .WithSuccessfulRemove(cacheKey);
            var userRepo = new UserRepositoryMockBuilder()
                .WithUserByEmail(email, user)
                .WithSuccessfulDatabaseUpdate();
            CreateHandler(hashingService.Object, userRepo.Object, cacheService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            hashingService.Mock.Verify(service => service.Hash(newPassword), Times.Once());
            cacheService.Mock.Verify(service => service.GetAsync<string>(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
            cacheService.Mock.Verify(service => service.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
            userRepo.Mock.Verify(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once());
            userRepo.Mock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once());
            user.Password.Should().Be(newPasswordHash);
        }

        [Fact]
        public async Task Handler_Should_ReturnOtpExpired_WhenUserIsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string username = "lynx_user";
            const string email = "user@lynx.com";
            const string newPassword = "NewPassword@13";
            const string oldPasswordHash = "VeryStrongPasswordName";
            const string otp = "132465";
            string cacheKey = $"reset_password_otps:{userId}";
            var user = User.Create(userId, email, username, oldPasswordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var request = new PasswordResetCommand()
            {
                Code = otp,
                Email = email,
                NewPassword = newPassword
            };
            var hashingService = new PasswordHashingServiceMockBuilder();
            var cacheService = new CacheServiceMockBuilder();
            var userRepo = new UserRepositoryMockBuilder().WithUserByEmail(email, null);
            CreateHandler(hashingService.Object, userRepo.Object, cacheService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.OtpExpired.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once());
            user.Password.Should().Be(oldPasswordHash);
        }

        [Fact]
        public async Task Handler_Should_ReturnOtpExpired_WhenOtpIsNotFoundInCache()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string username = "lynx_user";
            const string email = "user@lynx.com";
            const string newPassword = "NewPassword@13";
            const string oldPasswordHash = "VeryStrongPasswordName";
            const string otp = "132465";
            string cacheKey = $"reset_password_otps:{userId}";
            var user = User.Create(userId, email, username, oldPasswordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var request = new PasswordResetCommand()
            {
                Code = otp,
                Email = email,
                NewPassword = newPassword
            };
            var hashingService = new PasswordHashingServiceMockBuilder();
            var cacheService = new CacheServiceMockBuilder().WithSuccessfulGet<string?>(cacheKey, null);
            var userRepo = new UserRepositoryMockBuilder().WithUserByEmail(email, user);
            CreateHandler(hashingService.Object, userRepo.Object, cacheService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.OtpExpired.Code);
            cacheService.Mock.Verify(service => service.GetAsync<string>(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
            userRepo.Mock.Verify(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once());
            user.Password.Should().Be(oldPasswordHash);
        }
    }
}
