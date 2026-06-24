using FluentAssertions;
using Lynx.RedirectionService.Application.Common.Errors;
using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Application.Features.Urls.Commands.DeleteUrl;
using Lynx.RedirectionService.Application.UnitTests.MockBuilders;
using Lynx.RedirectionService.Domain.Common.Results;
using Lynx.RedirectionService.Domain.Urls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests
{
    public class DeleteUrlCommandTests
    {
        private readonly Mock<ILogger<DeleteUrlCommandHandler>> _logger = new(MockBehavior.Loose);
        private DeleteUrlCommandHandler? _handler;
        private readonly FakeTimeProvider _timeProvider;

        public DeleteUrlCommandTests()
        {
            _timeProvider = new FakeTimeProvider();
            _timeProvider.SetUtcNow(new DateTimeOffset(2026, 6, 24, 6, 13, 13, TimeSpan.Zero));
        }
        private void CreateHandler(IUrlRepository urlRepo)
        {
            _handler = new DeleteUrlCommandHandler(urlRepo, _logger.Object);
        }

        [Fact]
        public async Task Handler_Should_ReturnDeleted_When_CommandIsValid()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid urlId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "alias";
            Url url = Url.Create(urlId, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(15), _timeProvider).Value;
            DeleteUrlCommand command = new()
            {
                UserId = userId,
                UrlId = urlId
            };
            var urlRepoMock = new UrlRepositoryMockBuilder()
                .UrlById(urlId, url)
                .WithSuccessfullDatabaseUpdate();
            CreateHandler(urlRepoMock.Object);

            // Act
            var result = await _handler!.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Result.Deleted);
            urlRepoMock.Mock.Verify(repo => repo.GetUrlByIdAsync(urlId, It.IsAny<CancellationToken>()), Times.Once());
            urlRepoMock.Mock.Verify(repo => repo.UpdateUrlAsync(url, It.IsAny<CancellationToken>()), Times.Once());
            url.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task Handler_Should_UrlNotOwned_When_UserIdIsNotTheOwnerId()
        {
            // Arrange
            Guid realUserId = Guid.NewGuid();
            Guid fakeUserId = Guid.NewGuid();
            Guid urlId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "alias";
            Url url = Url.Create(urlId, realUserId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(15), _timeProvider).Value;
            DeleteUrlCommand command = new()
            {
                UserId = fakeUserId,
                UrlId = urlId
            };
            var urlRepoMock = new UrlRepositoryMockBuilder()
                .UrlById(urlId, url);
            CreateHandler(urlRepoMock.Object);

            // Act
            var result = await _handler!.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UrlNotOwned.Code);
            urlRepoMock.Mock.Verify(repo => repo.GetUrlByIdAsync(urlId, It.IsAny<CancellationToken>()), Times.Once());
            urlRepoMock.Mock.Verify(repo => repo.UpdateUrlAsync(url, It.IsAny<CancellationToken>()), Times.Never());
            url.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task Handler_Should_UrlDoesNotExist_When_UrlNotFound()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid realUrlId = Guid.NewGuid();
            Guid fakeUrlId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "alias";
            Url url = Url.Create(realUrlId, userId, longUrl, alias, _timeProvider.GetUtcNow().AddDays(15), _timeProvider).Value;
            DeleteUrlCommand command = new()
            {
                UserId = userId,
                UrlId = fakeUrlId
            };
            var urlRepoMock = new UrlRepositoryMockBuilder()
                .UrlById(fakeUrlId, null);
            CreateHandler(urlRepoMock.Object);

            // Act
            var result = await _handler!.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UrlDoesNotExist.Code);
            urlRepoMock.Mock.Verify(repo => repo.GetUrlByIdAsync(fakeUrlId, It.IsAny<CancellationToken>()), Times.Once());
            urlRepoMock.Mock.Verify(repo => repo.UpdateUrlAsync(url, It.IsAny<CancellationToken>()), Times.Never());
            url.IsDeleted.Should().BeFalse();
        }
    }
}
