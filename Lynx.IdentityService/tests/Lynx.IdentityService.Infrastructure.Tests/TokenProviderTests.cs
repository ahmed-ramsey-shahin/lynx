using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using FluentAssertions;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Infrastructure.Services;
using Lynx.IdentityService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.IdentityService.Infrastructure.Tests
{
    public class TokenProviderTests
    {
        private readonly TokenProvider _provider;
        private readonly FakeTimeProvider _timeProvider;
        private readonly Mock<IOTPGeneratorService> _generatorService;
        private readonly JwtSettings _jwtSettings;
        private readonly string _validPrivatePem;

        public TokenProviderTests()
        {
            using var rsa = RSA.Create();
            _validPrivatePem = rsa.ExportRSAPrivateKeyPem();
            _jwtSettings = new JwtSettings
            {
                PrivateKey = _validPrivatePem,
                Issuer = "LynxTestIssuer",
                Audience = "LynxTestAudience",
                ExpiryMinutes = 15
            };
            _timeProvider = new FakeTimeProvider();
            _generatorService = new(MockBehavior.Strict);
            _provider = new(Options.Create(_jwtSettings), _timeProvider, _generatorService.Object);
        }

        [Fact]
        public void GenerateJwtToken_Should_ReturnValidTokenWithCorrectClaims()
        {
            // Arrange
            var userDto = new UserDto
            {
                UserId = Guid.NewGuid(),
                Email = "user@lynx.com",
                Username = "lynx_user"
            };
            const string expectedRefreshToken = "mocked_refresh_token";
            _generatorService.Setup(service => service.GenerateUrlSafeToken(64)).Returns(expectedRefreshToken);
            var now = new DateTimeOffset(2026, 5, 27, 16, 0, 0, TimeSpan.Zero);
            _timeProvider.SetUtcNow(now);
            var expectedExpiresAt = now.AddMinutes(_jwtSettings.ExpiryMinutes);

            // Act
            var accessTokenDto = _provider.GenerateJwtToken(userDto);

            // Assert
            accessTokenDto.Should().NotBeNull();
            accessTokenDto.ExpiresAt.Should().Be(expectedExpiresAt);
            accessTokenDto.RefreshToken.Should().Be(expectedRefreshToken);
            _generatorService.Verify(service => service.GenerateUrlSafeToken(64), Times.Once());
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessTokenDto.AccessToken);
            jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
            jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
            jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?
                .Value.Should().NotBeNull().And.Be(userDto.UserId.ToString());
            jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?
                .Value.Should().NotBeNull().And.Be(userDto.Email);
            jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?
                .Value.Should().NotBeNull().And.Be(userDto.Username);
        }
    }
}
