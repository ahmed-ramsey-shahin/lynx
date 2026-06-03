using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using FluentAssertions;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Infrastructure.Services;
using Lynx.IdentityService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
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
                ExpiryInMinutes = 15
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
            var expectedExpiresAt = now.AddMinutes(_jwtSettings.ExpiryInMinutes);

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
            jwtToken.SignatureAlgorithm.Should().Be(SecurityAlgorithms.RsaSha256);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPrincipalFromExpiredToken_Should_ReturnTheCorrectPrincipalIfTheTokenIsExpiredOrNot(bool isExpired)
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
            var timeAfterExpiration = now.AddMinutes(_jwtSettings.ExpiryInMinutes + 5);
            _timeProvider.SetUtcNow(now);
            var expectedExpiresAt = now.AddMinutes(_jwtSettings.ExpiryInMinutes);
            var accessTokenDto = _provider.GenerateJwtToken(userDto);

            if (isExpired)
            {
                _timeProvider.SetUtcNow(timeAfterExpiration);
            }

            // Act
            var principal = _provider.GetPrincipalFromExpiredToken(accessTokenDto!.AccessToken);

            // Assert
            principal.Should().NotBeNull();
            principal!.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                .Should().NotBeNull().And.Be(userDto.UserId.ToString());
            principal!.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value
                .Should().NotBeNull().And.Be(userDto.Username);
            principal!.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
                .Should().NotBeNull().And.Be(userDto.Email);
        }

        [Fact]
        public void GetPublicKeyJwk_Should_ReturnCorrectPublickKey()
        {
            // Act
            var jwk = _provider.GetPublicKeyJwk();

            // Assert
            jwk.Should().NotBeNull();
            jwk.Kty.Should().Be("RSA");
            jwk.Alg.Should().Be(SecurityAlgorithms.RsaSha256);
            jwk.Use.Should().Be("sig");
            jwk.KeyId.Should().Be("lynx-auth-key-1");
            jwk.N.Should().NotBeNull();
            jwk.E.Should().NotBeNull();
        }
    }
}
