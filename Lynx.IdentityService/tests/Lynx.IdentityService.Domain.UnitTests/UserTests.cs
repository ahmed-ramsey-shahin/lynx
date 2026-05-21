using FluentAssertions;
using Lynx.IdentityService.Domain.Identity;

namespace Lynx.IdentityService.Domain.UnitTests
{
    public class UserTests
    {
        [Fact]
        public void Create_Should_ReturnUserObject_WhenParametersAreValid()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            const string email = "email@lynx.com";
            const string username = "lynx_user";
            const string password = "VeryStrong@Password123";

            // Act
            var userCreationResult = User.Create(id, email, username, password);
            var user = userCreationResult.Value;

            // Assert
            userCreationResult.IsSuccess.Should().BeTrue();
            userCreationResult.IsError.Should().BeFalse();
            userCreationResult.Errors.Should().BeEmpty();
            userCreationResult.Value.Should().NotBeNull();
            user.IsActivated.Should().BeFalse();
            user.ActivationDate.Should().BeNull();
            user.Email.Should().Be(email);
            user.Password.Should().Be(password);
            user.Username.Should().Be(username);
            user.Id.Should().Be(id);
            user.RefreshTokens.Should().BeEmpty();
            user.Events.Should().ContainSingle()
                .Which.Should().BeOfType<UserRegisteredEvent>();
        }

        [Fact]
        public void Create_Should_ReturnIdRequired_WhenIdIsEmpty()
        {
            // Arrange
            Guid id = Guid.Empty;
            const string email = "email@lynx.com";
            const string username = "lynx_user";
            const string password = "VeryStrong@Password123";

            // Act
            var userCreationResult = User.Create(id, email, username, password);

            // Assert
            userCreationResult.IsSuccess.Should().BeFalse();
            userCreationResult.IsError.Should().BeTrue();
            userCreationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.IdRequired.Code);
            userCreationResult.Value.Should().BeNull();
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(null)]
        public void Create_Should_ReturnEmailRequired_WhenEmailIsEmpty(string email)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            const string username = "lynx_user";
            const string password = "VeryStrong@Password123";

            // Act
            var userCreationResult = User.Create(id, email, username, password);

            // Assert
            userCreationResult.IsSuccess.Should().BeFalse();
            userCreationResult.IsError.Should().BeTrue();
            userCreationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.EmailRequired.Code);
            userCreationResult.Value.Should().BeNull();
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(null)]
        public void Create_Should_ReturnUsernameRequired_WhenUsernameIsEmpty(string username)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            const string email = "email@lynx.com";
            const string password = "VeryStrong@Password123";

            // Act
            var userCreationResult = User.Create(id, email, username, password);

            // Assert
            userCreationResult.IsSuccess.Should().BeFalse();
            userCreationResult.IsError.Should().BeTrue();
            userCreationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.UsernameRequired.Code);
            userCreationResult.Value.Should().BeNull();
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(null)]
        public void Create_Should_ReturnPasswordRequired_WhenPasswordIsEmpty(string password)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            const string email = "email@lynx.com";
            const string username = "lynx_user";

            // Act
            var userCreationResult = User.Create(id, email, username, password);

            // Assert
            userCreationResult.IsSuccess.Should().BeFalse();
            userCreationResult.IsError.Should().BeTrue();
            userCreationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.PasswordRequired.Code);
            userCreationResult.Value.Should().BeNull();
        }

        // ChangePassword(string password)

        // ChangeUsername(string username)

        // Activate(DateTimeOffset activationDate)

        // Delete()

        // AddRefreshToken(string token, DateTimeOffset expiresOn)

        // Revoke(string token, DateTimeOffset currnetUtcTime)

        // RevokeAllTokens(DateTimeOffset currnetUtcTime)

        // RemoveExpiredRefreshTokens(DateTimeOffset currentUtcTime)

        // RemoveRefreshToken(string token)
    }
}
