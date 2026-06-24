using FluentAssertions;
using Lynx.RedirectionService.Application.Common.Errors;
using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Application.Features.Urls.Dtos;
using Lynx.RedirectionService.Application.Features.Urls.Queries.GetUrlByAlias;
using Lynx.RedirectionService.Application.UnitTests.MockBuilders;
using Lynx.RedirectionService.Domain.Urls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests
{
    public class GetUrlByAliasQueryTests
    {
        private readonly Mock<ILogger<GetUrlByAliasQueryHandler>> _logger = new(MockBehavior.Loose);
        private readonly FakeTimeProvider _timeProvider;
        private readonly string queueName = "ShortLinkVisits";
        private GetUrlByAliasQueryHandler? _handler;

        public GetUrlByAliasQueryTests()
        {
            _timeProvider = new FakeTimeProvider();
            _timeProvider.SetUtcNow(new DateTimeOffset(2026, 6, 24, 6, 29, 53, TimeSpan.Zero));
        }

        private void CreateHandler(
            IUrlRepository urlRepo,
            IMessagePublishingService messagePublishingService
        )
        {
            _handler = new(urlRepo, _logger.Object, _timeProvider, messagePublishingService);
        }

        [Fact]
        public async Task Handler_Should_ReturnValidUrl_WhenQueryIsValid()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid urlId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "alias";
            Url url = Url.Create(urlId, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(15), _timeProvider).Value;
            GetUrlByAliasQuery query = new()
            {
                Alias = alias
            };
            var urlDto = new UrlDto
            {
                Id = url.Id,
                Alias = url.Alias,
                LongUrl = url.LongUrl,
                ExpiresAt = url.ExpirationDate
            };
            var urlRepoMock = new UrlRepositoryMockBuilder()
                .UrlByAlias(alias, url);
            var messagePublishingServiceMock = new MessagePublishingServiceMockBuilder()
                .WithSuccessfulPublish(queueName, urlDto);
            CreateHandler(urlRepoMock.Object, messagePublishingServiceMock.Object);

            // Act
            var result = await _handler!.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            urlRepoMock.Mock.Verify(repo => repo.GetUrlByAliasAsync(alias, It.IsAny<CancellationToken>()), Times.Once());
            messagePublishingServiceMock.Mock.Verify(service => service.PublishAsync(queueName, urlDto, It.IsAny<CancellationToken>()), Times.Once());
            result.Value.Should().Be(urlDto);
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        public async Task Handler_Should_ReturnUrlDoesNotExist_WhenUrlIsDeletedOrExpired(bool deleted, bool expired, bool empty)
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid urlId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "alias";
            Url url = Url.Create(urlId, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(3), _timeProvider).Value;

            if (expired)
            {
                _timeProvider.SetUtcNow(_timeProvider.GetUtcNow().AddDays(50));
            }

            if (deleted)
            {
                url.Delete();
            }

            GetUrlByAliasQuery query = new()
            {
                Alias = alias
            };
            var urlRepoMock = new UrlRepositoryMockBuilder()
                .UrlByAlias(alias, empty ? null : url);
            var messagePublishingServiceMock = new MessagePublishingServiceMockBuilder();
            CreateHandler(urlRepoMock.Object, messagePublishingServiceMock.Object);

            // Act
            var result = await _handler!.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UrlDoesNotExist.Code);
            urlRepoMock.Mock.Verify(repo => repo.GetUrlByAliasAsync(alias, It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
