using FluentAssertions;
using Lynx.RedirectionService.Domain.Urls;

namespace Lynx.RedirectionService.Domain.UnitTests
{
    public class UrlTests
    {
        [Fact]
        public void UrlCreate_Should_ReturnValidUrl_When_ParametersAreValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var longUrl = "very_long_url";
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, longUrl, alias, DateTimeOffset.UtcNow);

            // Assert
            creationResult.Should().NotBeNull();
            creationResult.Value.Id.Should().Be(id);
            creationResult.Value.LongUrl.Should().Be(longUrl);
            creationResult.Value.Alias.Should().Be(alias);
        }

        [Fact]
        public void UrlCreate_Should_ReturnIdRequiredError_When_IdIsEmpty()
        {
            // Arrange
            var id = Guid.Empty;
            var longUrl = "very_long_url";
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, longUrl, alias, DateTimeOffset.UtcNow);

            // Assert
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UrlErrors.IdRequired.Code);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void UrlCreate_Should_ReturnUrlRequiredError_When_UrlIsEmptyOrWhiteSpace(string longUrl)
        {
            // Arrange
            var id = Guid.NewGuid();
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, longUrl, alias, DateTimeOffset.UtcNow);

            // Assert
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UrlErrors.UrlRequired.Code);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void UrlCreate_Should_ReturnAliasRequiredError_When_AliasIsEmptyOrWhiteSpace(string alias)
        {
            // Arrange
            var id = Guid.NewGuid();
            var longUrl = "very_long_url";

            // Act
            var creationResult = Url.Create(id, longUrl, alias, DateTimeOffset.UtcNow);

            // Assert
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UrlErrors.AliasRequired.Code);
        }

        [Fact]
        public void UrlDelete_Should_DeleteUrl()
        {
            // Arrange
            var id = Guid.NewGuid();
            var longUrl = "very_long_url";
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, longUrl, alias, DateTimeOffset.UtcNow);
            creationResult.Value.Delete();

            // Assert
            creationResult.Value.IsDeleted.Should().BeTrue();
        }
    }
}
