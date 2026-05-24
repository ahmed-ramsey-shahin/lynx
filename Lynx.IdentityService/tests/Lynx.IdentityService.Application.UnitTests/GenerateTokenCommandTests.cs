using FluentAssertions;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Commands.GenerateToken;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class GenerateTokenCommandTests
    {
        private readonly Mock<ILogger<GenerateTokenCommandHandler>> _logger = new(MockBehavior.Loose);
        private GenerateTokenCommandHandler? _handler;

        private void CreateHandler(
            ITokenProvider tokenProvider,
            IUserRepository userRepository,
            IPasswordHashingService hashingService,
            TimeProvider timeProvider
        )
        {
            _handler = new(
                _logger.Object,
                tokenProvider,
                userRepository,
                hashingService,
                timeProvider
            );
        }

        [Fact]
        public async Task Handler_Should_ReturnTokenDto_WhenRequestIsValid()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            const string username = "lynx_user";
            const string password = "StrongPassword@123";
            const string email = "user@lynx.com";
            const string passwordHash = "StrongPassword@123_Hashed";
            const string accessToken = "AccessToken";
            const string refreshToken = "refreshToken";
            const string oldRefreshToken = "old-token";
            GenerateTokenCommand request = new()
            {
                Username = username,
                Password = password
            };
            User user = User.Create(userId, email, username, passwordHash).Value;
            FakeTimeProvider timeProvider = new();
            DateTimeOffset utcNow = new(2026, 5, 24, 9, 3, 0, TimeSpan.Zero);
            DateTimeOffset expiresAt = utcNow.AddHours(2);
            timeProvider.SetUtcNow(utcNow);
            user.Activate(timeProvider.GetUtcNow());
            user.AddRefreshToken(oldRefreshToken, utcNow.AddMinutes(-15));
            var tokenDto = new TokenDto()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };
            var userDto = new UserDto
            {
                Email = email,
                UserId = userId,
                Username = username
            };
            var tokenProvider = new TokenProviderMockBuilder().WithJwtToken(userDto, tokenDto);
            var userRepo = new UserRepositoryMockBuilder()
                .WithUserByUsername(username, user)
                .WithSuccessfulDatabaseUpdate();
            var hashingService = new PasswordHashingServiceMockBuilder().WithSuccessfulVerify(password);
            CreateHandler(tokenProvider.Object, userRepo.Object, hashingService.Object, timeProvider);

            // Act
           var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            tokenProvider.Mock.Verify(provider => provider.GenerateJwtToken(userDto), Times.Once());
            hashingService.Mock.Verify(service => service.Verify(password, passwordHash), Times.Once());
            userRepo.Mock.Verify(repo => repo.GetUserByUsernameAsync(username, It.IsAny<CancellationToken>()), Times.Once());
            userRepo.Mock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once());
            user.RefreshTokens.Should().ContainSingle();
            user.RefreshTokens.Should().ContainEquivalentOf(new { Token = refreshToken });
            user.RefreshTokens.Should().NotContainEquivalentOf(new { Token = oldRefreshToken });
        }

        [Fact]
        public async Task Handler_Should_ReturnInvalidCredentials_WhenUsernameIsNotCorrect()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            const string username = "lynx_user";
            const string password = "StrongPassword@123";
            GenerateTokenCommand request = new()
            {
                Username = username,
                Password = password
            };
            FakeTimeProvider timeProvider = new();
            DateTimeOffset utcNow = new(2026, 5, 24, 9, 3, 0, TimeSpan.Zero);
            timeProvider.SetUtcNow(utcNow);
            var tokenProvider = new TokenProviderMockBuilder();
            var userRepo = new UserRepositoryMockBuilder().WithUserByUsername(username, null);
            var hashingService = new PasswordHashingServiceMockBuilder();
            CreateHandler(tokenProvider.Object, userRepo.Object, hashingService.Object, timeProvider);

            // Act
           var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.CredentialsInvalid.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByUsernameAsync(username, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnInvalidCredentials_WhenPasswordIsNotCorrect()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            const string username = "lynx_user";
            const string password = "StrongPassword@123";
            const string email = "user@lynx.com";
            const string passwordHash = "StrongPassword@123_WrongHashed";
            const string accessToken = "AccessToken";
            const string refreshToken = "refreshToken";
            const string oldRefreshToken = "old-token";
            GenerateTokenCommand request = new()
            {
                Username = username,
                Password = password
            };
            User user = User.Create(userId, email, username, passwordHash).Value;
            FakeTimeProvider timeProvider = new();
            DateTimeOffset utcNow = new(2026, 5, 24, 9, 3, 0, TimeSpan.Zero);
            DateTimeOffset expiresAt = utcNow.AddHours(2);
            timeProvider.SetUtcNow(utcNow);
            user.Activate(timeProvider.GetUtcNow());
            user.AddRefreshToken(oldRefreshToken, utcNow.AddMinutes(-15));
            var tokenDto = new TokenDto()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };
            var userDto = new UserDto
            {
                Email = email,
                UserId = userId,
                Username = username
            };
            var tokenProvider = new TokenProviderMockBuilder();
            var userRepo = new UserRepositoryMockBuilder().WithUserByUsername(username, user);
            var hashingService = new PasswordHashingServiceMockBuilder().WithFailedVerify(password);
            CreateHandler(tokenProvider.Object, userRepo.Object, hashingService.Object, timeProvider);

            // Act
           var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.CredentialsInvalid.Code);
            hashingService.Mock.Verify(service => service.Verify(password, passwordHash), Times.Once());
            userRepo.Mock.Verify(repo => repo.GetUserByUsernameAsync(username, It.IsAny<CancellationToken>()), Times.Once());
            user.RefreshTokens.Should().ContainSingle();
            user.RefreshTokens.Should().ContainEquivalentOf(new { Token = oldRefreshToken });
        }

        [Fact]
        public async Task Handler_Should_ReturnTokenGenerationFailed_WhenGenerateTokenReturnsNull()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            const string username = "lynx_user";
            const string password = "StrongPassword@123";
            const string email = "user@lynx.com";
            const string passwordHash = "StrongPassword@123_Hashed";
            const string oldRefreshToken = "old-token";
            GenerateTokenCommand request = new()
            {
                Username = username,
                Password = password
            };
            User user = User.Create(userId, email, username, passwordHash).Value;
            FakeTimeProvider timeProvider = new();
            DateTimeOffset utcNow = new(2026, 5, 24, 9, 3, 0, TimeSpan.Zero);
            DateTimeOffset expiresAt = utcNow.AddHours(2);
            timeProvider.SetUtcNow(utcNow);
            user.Activate(timeProvider.GetUtcNow());
            user.AddRefreshToken(oldRefreshToken, utcNow.AddMinutes(-15));
            var userDto = new UserDto
            {
                Email = email,
                UserId = userId,
                Username = username
            };
            var tokenProvider = new TokenProviderMockBuilder().WithJwtToken(userDto, null);
            var userRepo = new UserRepositoryMockBuilder().WithUserByUsername(username, user);
            var hashingService = new PasswordHashingServiceMockBuilder().WithSuccessfulVerify(password);
            CreateHandler(tokenProvider.Object, userRepo.Object, hashingService.Object, timeProvider);

            // Act
           var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.TokenGenerationFailed.Code);
            tokenProvider.Mock.Verify(provider => provider.GenerateJwtToken(userDto), Times.Once());
            hashingService.Mock.Verify(service => service.Verify(password, passwordHash), Times.Once());
            userRepo.Mock.Verify(repo => repo.GetUserByUsernameAsync(username, It.IsAny<CancellationToken>()), Times.Once());
            user.RefreshTokens.Should().ContainSingle();
            user.RefreshTokens.Should().ContainEquivalentOf(new { Token = oldRefreshToken });
        }
    }
}
