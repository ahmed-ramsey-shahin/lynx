using FluentAssertions;
using Lynx.RedirectionService.Domain.Urls;
using Microsoft.Extensions.Time.Testing;

namespace Lynx.RedirectionService.Domain.UnitTests
{
    public class UrlTests
    {
        private readonly FakeTimeProvider _timeProvider;

        public UrlTests()
        {
            _timeProvider = new FakeTimeProvider();
            _timeProvider.SetUtcNow(new DateTimeOffset(2026, 6, 19, 6, 26, 30, TimeSpan.Zero));
        }

        [Fact]
        public void UrlCreate_Should_ReturnValidUrl_When_ParametersAreValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var longUrl = "very_long_url";
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(1), _timeProvider);

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
            var userId = Guid.NewGuid();
            var longUrl = "very_long_url";
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(1), _timeProvider);

            // Assert
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UrlErrors.IdRequired.Code);
        }

        [Fact]
        public void UrlCreate_Should_ReturnUserIdRequiredError_When_UserIdIsEmpty()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.Empty;
            var longUrl = "very_long_url";
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(1), _timeProvider);

            // Assert
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UrlErrors.UserIdRequired.Code);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void UrlCreate_Should_ReturnUrlRequiredError_When_UrlIsEmptyOrWhiteSpace(string longUrl)
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(1), _timeProvider);

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
            var userId = Guid.NewGuid();
            var longUrl = "very_long_url";

            // Act
            var creationResult = Url.Create(id, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(1), _timeProvider);

            // Assert
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UrlErrors.AliasRequired.Code);
        }

        [Fact]
        public void UrlCreate_Should_ReturnExpirartionDateInvalid_When_ExpirationDateIsInThePast()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var longUrl = "very_long_url";
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, userId, longUrl, alias, _timeProvider.GetUtcNow().AddMinutes(-1), _timeProvider);

            // Assert
            creationResult.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UrlErrors.ExpirationDateInvalid.Code);
        }

        [Fact]
        public void UrlDelete_Should_DeleteUrl()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var longUrl = "very_long_url";
            var alias = "short_alias";

            // Act
            var creationResult = Url.Create(id, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(1), _timeProvider);
            creationResult.Value.Delete();

            // Assert
            creationResult.Value.IsDeleted.Should().BeTrue();
        }
    }
}
