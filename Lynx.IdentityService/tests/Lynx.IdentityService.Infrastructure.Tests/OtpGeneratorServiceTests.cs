using FluentAssertions;
using Lynx.IdentityService.Infrastructure.Services;

namespace Lynx.IdentityService.Infrastructure.Tests
{
    public class OtpGeneratorServiceTests
    {
        private readonly OtpGeneratorService? _generatorService = new();

        [Fact]
        public void GenerateResetCode_Should_ReturnUniqueCodes()
        {
            // Arrange
            HashSet<string> set = [];
            const int numberOfTests = 1000;

            // Act
            for (int i = 0; i < numberOfTests; i++)
            {
                set.Add(_generatorService!.GenerateResetCode());
            }

            // Assert
            set.Should().HaveCount(numberOfTests);
        }

        [Fact]
        public void GenerateResetCode_Should_Return6DigitsString()
        {
            for (int i = 0; i < 100; i++)
            {
                _generatorService!.GenerateResetCode().Should().MatchRegex("^[0-9]{6}$");
            }
        }

        [Fact]
        public void GenerateUrlSafeToken_Should_ReturnUniqueCodes()
        {
            // Arrange
            HashSet<string> set = [];
            const int numberOfTests = 1000;

            // Act
            for (int i = 0; i < numberOfTests; i++)
            {
                set.Add(_generatorService!.GenerateUrlSafeToken());
            }

            // Assert
            set.Should().HaveCount(numberOfTests);
        }

        [Fact]
        public void GenerateUrlSafeToken_Should_Return32DigitHexNumber()
        {
            for (int i = 0; i < 100; i++)
            {
                _generatorService!.GenerateUrlSafeToken().Should().MatchRegex("^[0-9a-fA-F]{32}$");
            }
        }
    }
}
