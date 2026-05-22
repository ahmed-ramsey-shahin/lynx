using FluentAssertions;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Commands.ActivateUser;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class ActivateUserCommandTests
    {
        private readonly Mock<ILogger<ActivateUserCommandHandler>> _logger = new(MockBehavior.Loose);
        private ActivateUserCommandHandler? _handler;

        private void CreateHandler(
            IUserRepository userRepo,
            ICacheService cacheService
        )
        {
            var timeProvider = new FakeTimeProvider();
            var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
            timeProvider.SetUtcNow(frozenTime);
            _handler = new ActivateUserCommandHandler(userRepo, cacheService, _logger.Object, timeProvider);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handler_Should_ReturnUpdatedRegardlessOfPreviousActivations_WhenRequestIsValid(bool alreadyActivated)
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string activationCode = "A Very Strong Very Random Activation Code";
            const string cacheKey = $"activation-codes:{activationCode}";
            var request = new ActivateUserCommand
            {
                ActivationCode = activationCode
            };
            var cache = new CacheServiceMockBuilder()
                .WithSuccessfulGet<Guid?>(cacheKey, userId)
                .WithSuccessfulRemove(cacheKey);
            var user = User.Create(userId, "user@lynx.com", "randomUsername", "randomPassword").Value;

            if (alreadyActivated)
            {
                user.Activate(DateTimeOffset.UtcNow);
            }

            var userRepo = new UserRepositoryMockBuilder()
                .WithUserById(userId, user)
                .WithSuccessfulDatabaseUpdate();
            CreateHandler(
                userRepo.Object,
                cache.Object
            );

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            userRepo.Mock.Verify(repo => repo.UpdateAsync(It.Is<User>(user =>
                user.Id == userId), It.IsAny<CancellationToken>()), Times.Once());
            cache.Mock.Verify(service => service.GetAsync<Guid?>(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
            cache.Mock.Verify(service => service.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Hadler_Should_ReturnActivationCodeExpired_WhenActivationCodeIsNotInCache()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string activationCode = "A Very Strong Very Random Activation Code";
            const string cacheKey = $"activation-codes:{activationCode}";
            var request = new ActivateUserCommand
            {
                ActivationCode = activationCode
            };
            var cache = new CacheServiceMockBuilder()
                .WithSuccessfulGet<Guid?>(cacheKey, null);
            var userRepo = new UserRepositoryMockBuilder();
            CreateHandler(
                userRepo.Object,
                cache.Object
            );

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.ActivationCodeExpired.Code);
            cache.Mock.Verify(service => service.GetAsync<Guid?>(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Hadler_Should_ReturnUserNotFound_WhenUserIdIsNotInRepo()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string activationCode = "A Very Strong Very Random Activation Code";
            const string cacheKey = $"activation-codes:{activationCode}";
            var request = new ActivateUserCommand
            {
                ActivationCode = activationCode
            };
            var cache = new CacheServiceMockBuilder()
                .WithSuccessfulGet<Guid?>(cacheKey, userId);
            var userRepo = new UserRepositoryMockBuilder()
                .WithUserById(userId, null);
            CreateHandler(
                userRepo.Object,
                cache.Object
            );

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UserNotFound.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            cache.Mock.Verify(service => service.GetAsync<Guid?>(cacheKey, It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
