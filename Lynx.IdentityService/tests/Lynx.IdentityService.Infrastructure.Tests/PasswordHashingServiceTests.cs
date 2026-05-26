using FluentAssertions;
using Lynx.IdentityService.Infrastructure.Services;

namespace Lynx.IdentityService.Infrastructure.Tests
{
    public class PasswordHashingServiceTests
    {
        private readonly PasswordHashingService _hashingService = new("random_pepper");

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Hash_Should_ThrowArgumentException_WhenPasswordIsEmpty(string password)
        {
            // Arrange
            Action action = () => _hashingService.Hash(password);

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Hash_Should_ReturnUniqueHasheForThePassword()
        {
            // Arrange
            const string password = "password";

            // Act
            var hash1 = _hashingService.Hash(password);
            var hash2 = _hashingService.Hash(password);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void Hash_Should_NotEqualPlainTextPassword()
        {
            // Arrange
            const string password = "password";

            // Act
            var hash = _hashingService.Hash(password);

            // Assert
            hash.Should().NotBe(password);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", " ")]
        [InlineData(" ", "")]
        [InlineData(" ", " ")]
        public void Verify_Should_ReturnFalse_WhenEitherPasswordOrHashIsEmpty(string password, string hash)
        {
            _hashingService.Verify(password, hash).Should().BeFalse();
        }

        [Fact]
        public void Verify_Should_ReturnTrue_WhenValidatingTheCorrectPassword()
        {
            // Arrange
            const string password = "password";

            // Act
            var hash = _hashingService.Hash(password);

            // Assert
            _hashingService.Verify(password, hash).Should().BeTrue();
        }

        [Fact]
        public void Verify_Should_ReturnFalse_WheValidatingTheWrongPassword()
        {
            // Arrange
            const string password = "password!";
            const string wrongPassword = "password?";

            // Act
            var hash = _hashingService.Hash(password);

            // Assert
            _hashingService.Verify(wrongPassword, hash).Should().BeFalse();
        }
    }
}
