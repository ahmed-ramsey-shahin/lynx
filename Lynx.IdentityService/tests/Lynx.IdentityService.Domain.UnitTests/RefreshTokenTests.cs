using FluentAssertions;
using Lynx.IdentityService.Domain.Common.Results;
using Lynx.IdentityService.Domain.Identity;

namespace Lynx.IdentityService.Domain.UnitTests
{
    public class RefreshTokenTests
    {
        [Fact]
        public void Create_Should_ReturnRefreshToken_WhenParametersAreValid()
        {
            // Arrange
            const string tokenString = "some_random_token";
            var expiresOn = DateTimeOffset.UtcNow.AddDays(7);

            // Act
            var creationResult = RefreshToken.Create(tokenString, expiresOn);

            // Assert
            creationResult.IsSuccess.Should().BeTrue();
            var refreshToken = creationResult.Value;
            refreshToken.Token.Should().Be(tokenString);
            refreshToken.ExpiresOn.Should().Be(expiresOn);
            refreshToken.IsRevoked.Should().BeFalse();
            refreshToken.RevokedAt.Should().BeNull();
            refreshToken.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_Should_ReturnTokenRequiredError_WhenTokenIsNullOrWhitespace(string token)
        {
            // Arrange
            var expiresOn = DateTimeOffset.UtcNow.AddDays(7);

            // Act
            var creationResult = RefreshToken.Create(token, expiresOn);

            // Assert
            creationResult.IsSuccess.Should().BeFalse();
            creationResult.IsError.Should().BeTrue();
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(RefreshTokenErrors.TokenRequired.Code);
        }

        [Fact]
        public void Create_Should_ReturnExpirationInvalidError_WhenExpiresOnIsInThePast()
        {
            // Arrange
            const string token = "RandomToken";
            var expiresOn = DateTimeOffset.UtcNow.AddDays(-1);

            // Act
            var creationResult = RefreshToken.Create(token, expiresOn);

            // Assert
            creationResult.IsSuccess.Should().BeFalse();
            creationResult.IsError.Should().BeTrue();
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(RefreshTokenErrors.ExpirationInvalid.Code);
        }

        [Fact]
        public void Revoke_Should_ChangeRevokedAtAndIsRevoked_WhenNotAlreadyRevoked()
        {
            // Arrange
            const string token = "RandomToken";
            var expiresOn = DateTimeOffset.UtcNow.AddHours(2);
            var creationResult = RefreshToken.Create(token, expiresOn);
            var refreshToken = creationResult.Value;
            var revokeTime = DateTimeOffset.UtcNow;

            // Act
            var revokeResult = refreshToken.Revoke(revokeTime);

            // Assert
            revokeResult.IsSuccess.Should().BeTrue();
            revokeResult.IsError.Should().BeFalse();
            revokeResult.Errors.Should().BeEmpty();
            revokeResult.Value.Should().Be(Result.Updated);
            refreshToken.IsRevoked.Should().BeTrue();
            refreshToken.RevokedAt.Should().Be(revokeTime);
        }

        [Fact]
        public void Revoke_Should_ReturnUpdatedAndNothingChanges_WhenAlreadyRevoked()
        {
            // Arrange
            const string token = "RandomToken";
            var expiresOn = DateTimeOffset.UtcNow.AddHours(2);
            var creationResult = RefreshToken.Create(token, expiresOn);
            var refreshToken = creationResult.Value;
            var revokeTime = DateTimeOffset.UtcNow;
            var reRevokeTime = DateTimeOffset.UtcNow.AddHours(3);
            refreshToken.Revoke(revokeTime);

            // Act
            var revokeResult = refreshToken.Revoke(reRevokeTime);

            // Assert
            revokeResult.IsSuccess.Should().BeTrue();
            revokeResult.IsError.Should().BeFalse();
            revokeResult.Errors.Should().BeEmpty();
            revokeResult.Value.Should().Be(Result.Updated);
            refreshToken.IsRevoked.Should().BeTrue();
            refreshToken.RevokedAt.Should().Be(revokeTime);
        }
    }
}
