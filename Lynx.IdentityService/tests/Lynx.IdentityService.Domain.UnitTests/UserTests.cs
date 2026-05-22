using FluentAssertions;
using Lynx.IdentityService.Domain.Identity;

namespace Lynx.IdentityService.Domain.UnitTests
{
    public class UserTests
    {
#region CREATE_TESTS
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
            userCreationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.IdRequired.Code);
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
            userCreationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.EmailRequired.Code);
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
            userCreationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.UsernameRequired.Code);
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
            userCreationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.PasswordRequired.Code);
        }
#endregion // CREATE_TESTS

#region CHANGE_PASSWORD_TESTS
        [Fact]
        public void ChangePassword_Should_ReturnUpdated_WhenParametersAreValidAndUserIsActivated()
        {
            // Arrange
            var user = new UserBuilder().Activated().Build();
            const string newPassword = "VeryStrong@NewPassword456";

            // Act
            var changePasswordResult = user.ChangePassword(newPassword);

            // Assert
            changePasswordResult.IsSuccess.Should().BeTrue();
            user.Password.Should().Be(newPassword);
            user.Events.Should().ContainSingle()
                .Which.Should().BeOfType<PasswordChangedEvent>();
        }

        [Fact]
        public void ChangePassword_Should_ReturnNotActivated_WhenUserIsNotActivated()
        {
            // Arrange
            var user = new UserBuilder().Build();
            string oldPassword = user.Password;
            const string newPassword = "VeryStrong@NewPassword456";

            // Act
            var changePasswordResult = user.ChangePassword(newPassword);

            // Assert
            changePasswordResult.IsSuccess.Should().BeFalse();
            changePasswordResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.NotActivated.Code);
            user.Password.Should().Be(oldPassword);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ChangePassword_Should_ReturnPasswordRequired_WhenNewPasswordIsEmpty(string newPassword)
        {
            // Arrange
            var user = new UserBuilder().Activated().Build();
            string oldPassword = user.Password;

            // Act
            var changePasswordResult = user.ChangePassword(newPassword);

            // Assert
            changePasswordResult.IsSuccess.Should().BeFalse();
            changePasswordResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.PasswordRequired.Code);
            user.Password.Should().Be(oldPassword);
        }
#endregion // CHANGE_PASSWORD_TESTS

#region CHANGE_USERNAME_TESTS
        [Fact]
        public void ChangeUsername_Should_ReturnUpdated_WhenParametersAreValidAndUserIsActivated()
        {
            // Arrange
            var user = new UserBuilder().Activated().Build();
            const string newUsername = "new_lynx_user";

            // Act
            var changeUsernameResult = user.ChangeUsername(newUsername);

            // Assert
            changeUsernameResult.IsSuccess.Should().BeTrue();
            user.Username.Should().Be(newUsername);
            user.Events.Should().ContainSingle()
                .Which.Should().BeOfType<UsernameChangedEvent>();
        }

        [Fact]
        public void ChangeUsername_Should_ReturnNotActivated_WhenUserIsNotActivated()
        {
            // Arrange
            var user = new UserBuilder().Build();
            string oldUsername = user.Username;
            const string newUsername = "new_lynx_user";

            // Act
            var changeUsernameResult = user.ChangeUsername(newUsername);

            // Assert
            changeUsernameResult.IsSuccess.Should().BeFalse();
            changeUsernameResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.NotActivated.Code);
            user.Username.Should().Be(oldUsername);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ChangeUsername_Should_ReturnUsernameRequired_WhenNewUsernameIsEmpty(string newUsername)
        {
            // Arrange
            var user = new UserBuilder().Activated().Build();
            string oldUsername = user.Username;

            // Act
            var changeUsernameResult = user.ChangeUsername(newUsername);

            // Assert
            changeUsernameResult.IsSuccess.Should().BeFalse();
            changeUsernameResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.UsernameRequired.Code);
            user.Username.Should().Be(oldUsername);
        }
#endregion // CHANGE_USERNAME_TESTS

#region ACTIVATE_TESTS
        [Fact]
        public void Activate_Should_ReturnUpdatedAndEvent_IfNotActivated()
        {
            // Arrange
            var user = new UserBuilder().Build();
            var activationTime = DateTimeOffset.UtcNow;

            // Act
            var activationResult = user.Activate(activationTime);

            // Assert
            activationResult.IsSuccess.Should().BeTrue();
            user.IsActivated.Should().BeTrue();
            user.ActivationDate.Should().Be(activationTime);
            user.Events.Should().ContainSingle()
                .Which.Should().BeOfType<UserActivatedEvent>();
        }

        [Fact]
        public void Activate_Should_ReturnUpdatedAndNoEvent_IfActivated()
        {
            // Arrange
            var user = new UserBuilder().Activated().Build();
            var oldActivationTime = user.ActivationDate;
            var newActivationTime = DateTimeOffset.UtcNow.AddMinutes(15);

            // Act
            var activationResult = user.Activate(newActivationTime);

            // Assert
            activationResult.IsSuccess.Should().BeTrue();
            user.IsActivated.Should().BeTrue();
            user.ActivationDate.Should().Be(oldActivationTime);
            user.Events.Should().BeEmpty();
        }
#endregion // ACTIVATE_TESTS

#region DELETE_TESTS
        [Fact]
        public void Delete_Should_MarkUserAsDeleted_WhenUserIsActivated()
        {
            // Arrange
            var user = new UserBuilder().Activated().Build();
            var deletionTime = DateTimeOffset.UtcNow;

            // Act
            var deletionResult = user.Delete(deletionTime);

            // Assert
            deletionResult.IsSuccess.Should().BeTrue();
            user.IsDeleted.Should().BeTrue();
            user.DeletedAt.Should().Be(deletionTime);
            user.Events.Should().ContainSingle()
                .Which.Should().BeOfType<UserDeletedEvent>();
        }

        [Fact]
        public void Delete_Should_ReturnNoActivated_WhenUserIsNotActivate()
        {
            // Arrange
            var user = new UserBuilder().Build();
            var deletionTime = DateTimeOffset.UtcNow;

            // Act
            var deletionResult = user.Delete(deletionTime);

            // Assert
            deletionResult.IsSuccess.Should().BeFalse();
            deletionResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.NotActivated.Code);
        }
#endregion // DELETE_TESTS

#region ADD_REFRESH_TOKEN_TESTS
        [Fact]
        public void AddRefreshToken_Should_ReturnUpdated_WhenActivated()
        {
            // Arrange
            var user = new UserBuilder().Activated().Build();
            const string token = "RandomToken";
            DateTimeOffset expiresOn = DateTimeOffset.UtcNow.AddMinutes(15);

            // Act
            var additionResult = user.AddRefreshToken(token, expiresOn);

            // Assert
            additionResult.IsSuccess.Should().BeTrue();
            var addedToken = user.RefreshTokens.Should().ContainSingle().Which;
            addedToken.Token.Should().Be(token);
            addedToken.ExpiresOn.Should().Be(expiresOn);
        }

        [Fact]
        public void AddRefreshToken_Should_ReturnNotActivated_WhenNotActivated()
        {
            // Arrange
            var user = new UserBuilder().Build();
            const string token = "RandomToken";
            DateTimeOffset expiresOn = DateTimeOffset.UtcNow.AddMinutes(15);

            // Act
            var additionResult = user.AddRefreshToken(token, expiresOn);

            // Assert
            additionResult.IsSuccess.Should().BeFalse();
            additionResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.NotActivated.Code);
            user.RefreshTokens.Should().BeEmpty();
        }

        [Fact]
        public void AddRefreshToken_Should_ReturnCreationError_WhenTokenDataIsInvalid()
        {
            // Arrange
            var user = new UserBuilder().Activated().Build();
            const string token = "";
            var expiresOn = DateTimeOffset.UtcNow;

            // Act
            var additionResult = user.AddRefreshToken(token, expiresOn);

            // Assert
            additionResult.IsSuccess.Should().BeFalse();
            additionResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(RefreshTokenErrors.TokenRequired.Code);
            user.RefreshTokens.Should().BeEmpty();
        }
#endregion // ADD_REFRESH_TOKEN_TESTS

        // Revoke(string token, DateTimeOffset currnetUtcTime)

        // RevokeAllTokens(DateTimeOffset currnetUtcTime)

        // RemoveExpiredRefreshTokens(DateTimeOffset currentUtcTime)

        // RemoveRefreshToken(string token)
    }
}
