using System.Security.Claims;
using FluentAssertions;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Commands.RefreshToken;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class RefreshTokenCommandTests
    {
        private readonly Mock<ILogger<RefreshTokenCommandHandler>> _logger = new(MockBehavior.Loose);
        private RefreshTokenCommandHandler? _handler;

        private void CreateHandler(
            IUserRepository userRepo,
            ITokenProvider tokenProvider,
            TimeProvider timeProvider
        )
        {
            _handler = new(_logger.Object, userRepo, tokenProvider, timeProvider);
        }

        [Fact]
        public async Task Handler_Should_ReturnTokenDto_WhenRequestIsValid()
        {
            // Arrange
            var timeProvider = new FakeTimeProvider();
            var utcNow = new DateTimeOffset(2026, 5, 24, 10, 28, 30, TimeSpan.Zero);
            timeProvider.SetUtcNow(utcNow);
            const string refreshToken = "refreshToken";
            const string expiredAccessToken = "expiredToken";
            const string newAccessToken = "newToken";
            const string expiredRefreshToken = "expiredRefreshToken";
            const string newRefreshToken = "newRefreshToken";
            var request = new RefreshTokenCommand()
            {
                RefreshToken = refreshToken,
                ExpiredAccessToken = expiredAccessToken
            };
            var userId = Guid.NewGuid();
            const string email = "user@lynx.com";
            const string username = "lynx_user";
            const string passwordHash = "VeryStrongPassword@123_Hashed";
            User user = User.Create(userId, email, username, passwordHash).Value;
            user.Activate(timeProvider.GetUtcNow());
            user.AddRefreshToken(refreshToken, utcNow.AddMinutes(30));
            user.AddRefreshToken(expiredRefreshToken, utcNow.AddMinutes(-60));
            List<Claim> claims = [
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            ];
            ClaimsIdentity identity = new(claims);
            ClaimsPrincipal principal = new(identity);
            var tokenProvider = new TokenProviderMockBuilder()
                .WithJwtToken(new UserDto
                {
                    Email = email,
                    UserId = userId,
                    Username = username
                }, new TokenDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = utcNow.AddHours(2)
                }).WithPrincipal(expiredAccessToken, principal);
            var userRepo = new UserRepositoryMockBuilder()
                .WithSuccessfulDatabaseUpdate()
                .WithUserById(userId, user);
            CreateHandler(userRepo.Object, tokenProvider.Object, timeProvider);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            userRepo.Mock.Verify(repo => repo.UpdateAsync(It.Is<User>(user => user.Id == userId), It.IsAny<CancellationToken>()), Times.Once());
            tokenProvider.Mock.Verify(provider => provider.GenerateJwtToken(It.Is<UserDto>(user => user.UserId == userId)), Times.Once());
            tokenProvider.Mock.Verify(provider => provider.GetPrincipalFromExpiredToken(expiredAccessToken), Times.Once());
            result.Value.AccessToken.Should().Be(newAccessToken);
            result.Value.RefreshToken.Should().Be(newRefreshToken);
            user.RefreshTokens.Should().ContainSingle()
                .Which.Token.Should().Be(newRefreshToken);
        }

        [Fact]
        public async Task Handler_Should_ReturnExpiredRefreshToken_WhenClaimsPrincipleIsNull()
        {
            // Arrange
            var timeProvider = new FakeTimeProvider();
            var utcNow = new DateTimeOffset(2026, 5, 24, 10, 28, 30, TimeSpan.Zero);
            timeProvider.SetUtcNow(utcNow);
            const string refreshToken = "refreshToken";
            const string expiredAccessToken = "expiredToken";
            const string expiredRefreshToken = "expiredRefreshToken";
            var request = new RefreshTokenCommand()
            {
                RefreshToken = refreshToken,
                ExpiredAccessToken = expiredAccessToken
            };
            var userId = Guid.NewGuid();
            const string email = "user@lynx.com";
            const string username = "lynx_user";
            const string passwordHash = "VeryStrongPassword@123_Hashed";
            User user = User.Create(userId, email, username, passwordHash).Value;
            user.Activate(timeProvider.GetUtcNow());
            user.AddRefreshToken(refreshToken, utcNow.AddMinutes(30));
            user.AddRefreshToken(expiredRefreshToken, utcNow.AddMinutes(-60));
            var tokenProvider = new TokenProviderMockBuilder().WithPrincipal(expiredAccessToken, null);
            var userRepo = new UserRepositoryMockBuilder();
            CreateHandler(userRepo.Object, tokenProvider.Object, timeProvider);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.ExpiredAccessTokenInvalid.Code);
            tokenProvider.Mock.Verify(provider => provider.GetPrincipalFromExpiredToken(expiredAccessToken), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnRefreshTokenExpired_WhenNoActiveRefreshTokenIsAvailable()
        {
            // Arrange
            var timeProvider = new FakeTimeProvider();
            var utcNow = new DateTimeOffset(2026, 5, 24, 10, 28, 30, TimeSpan.Zero);
            timeProvider.SetUtcNow(utcNow);
            const string refreshToken = "refreshToken";
            const string expiredAccessToken = "expiredToken";
            const string expiredRefreshToken = "expiredRefreshToken";
            var request = new RefreshTokenCommand()
            {
                RefreshToken = refreshToken,
                ExpiredAccessToken = expiredAccessToken
            };
            var userId = Guid.NewGuid();
            const string email = "user@lynx.com";
            const string username = "lynx_user";
            const string passwordHash = "VeryStrongPassword@123_Hashed";
            User user = User.Create(userId, email, username, passwordHash).Value;
            user.Activate(timeProvider.GetUtcNow());
            user.AddRefreshToken(refreshToken, utcNow.AddSeconds(-1));
            user.AddRefreshToken(expiredRefreshToken, utcNow.AddMinutes(-60));
            List<Claim> claims = [
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            ];
            ClaimsIdentity identity = new(claims);
            ClaimsPrincipal principal = new(identity);
            var tokenProvider = new TokenProviderMockBuilder().WithPrincipal(expiredAccessToken, principal);
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, user);
            CreateHandler(userRepo.Object, tokenProvider.Object, timeProvider);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.RefreshTokenExpired.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            tokenProvider.Mock.Verify(provider => provider.GetPrincipalFromExpiredToken(expiredAccessToken), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnUserNotFound_WhenPrincipalsIdIsWrong()
        {
            // Arrange
            var timeProvider = new FakeTimeProvider();
            var utcNow = new DateTimeOffset(2026, 5, 24, 10, 28, 30, TimeSpan.Zero);
            timeProvider.SetUtcNow(utcNow);
            const string refreshToken = "refreshToken";
            const string expiredAccessToken = "expiredToken";
            var request = new RefreshTokenCommand()
            {
                RefreshToken = refreshToken,
                ExpiredAccessToken = expiredAccessToken
            };
            var userId = Guid.NewGuid();
            var wrongId = Guid.NewGuid();
            const string email = "user@lynx.com";
            const string username = "lynx_user";
            List<Claim> claims = [
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, wrongId.ToString()),
            ];
            ClaimsIdentity identity = new(claims);
            ClaimsPrincipal principal = new(identity);
            var tokenProvider = new TokenProviderMockBuilder().WithPrincipal(expiredAccessToken, principal);
            var userRepo = new UserRepositoryMockBuilder().WithUserById(wrongId, null);
            CreateHandler(userRepo.Object, tokenProvider.Object, timeProvider);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UserNotFound.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(wrongId, It.IsAny<CancellationToken>()), Times.Once());
            tokenProvider.Mock.Verify(provider => provider.GetPrincipalFromExpiredToken(expiredAccessToken), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnRefreshTokenExpired_WhenRefreshTokenIsWrong()
        {
            // Arrange
            var timeProvider = new FakeTimeProvider();
            var utcNow = new DateTimeOffset(2026, 5, 24, 10, 28, 30, TimeSpan.Zero);
            timeProvider.SetUtcNow(utcNow);
            const string refreshToken = "refreshToken";
            const string expiredAccessToken = "expiredToken";
            const string expiredRefreshToken = "expiredRefreshToken";
            const string wrongRefreshToken = "wrongRefreshToken";
            var request = new RefreshTokenCommand()
            {
                RefreshToken = wrongRefreshToken,
                ExpiredAccessToken = expiredAccessToken
            };
            var userId = Guid.NewGuid();
            const string email = "user@lynx.com";
            const string username = "lynx_user";
            const string passwordHash = "VeryStrongPassword@123_Hashed";
            User user = User.Create(userId, email, username, passwordHash).Value;
            user.Activate(timeProvider.GetUtcNow());
            user.AddRefreshToken(refreshToken, utcNow.AddMinutes(30));
            user.AddRefreshToken(expiredRefreshToken, utcNow.AddMinutes(-60));
            List<Claim> claims = [
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            ];
            ClaimsIdentity identity = new(claims);
            ClaimsPrincipal principal = new(identity);
            var tokenProvider = new TokenProviderMockBuilder().WithPrincipal(expiredAccessToken, principal);
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, user);
            CreateHandler(userRepo.Object, tokenProvider.Object, timeProvider);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.RefreshTokenExpired.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            tokenProvider.Mock.Verify(provider => provider.GetPrincipalFromExpiredToken(expiredAccessToken), Times.Once());
        }
    }
}
