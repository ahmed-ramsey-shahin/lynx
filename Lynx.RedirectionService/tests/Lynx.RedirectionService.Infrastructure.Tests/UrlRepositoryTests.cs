using Lynx.RedirectionService.Domain.Common;
using Lynx.RedirectionService.Domain.Urls;
using Lynx.RedirectionService.Infrastructure.Data.Configuration;
using Lynx.RedirectionService.Infrastructure.Data;
using Lynx.RedirectionService.Infrastructure.Tests.Fixtures;
using MediatR;
using Microsoft.Extensions.Time.Testing;
using MongoDB.Driver;
using Moq;
using FluentAssertions;

namespace Lynx.RedirectionService.Infrastructure.Tests
{
    [Collection("DatabaseCollection")]
    public class UrlRepositoryTests
    {
        private readonly IMongoDatabase _database;
        private readonly UrlRepository _urlRepository;
        private readonly FakeTimeProvider _timeProvider;
        private readonly Mock<IPublisher> _publisherMock = new(MockBehavior.Strict);

        public UrlRepositoryTests(DatabaseFixture fixture)
        {
            _publisherMock.Setup(mock => mock.Publish(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _timeProvider = new FakeTimeProvider();
            _timeProvider.SetUtcNow(new DateTimeOffset(2026, 6, 24, 10, 23, 59, TimeSpan.Zero));
            _urlRepository = new UrlRepository(fixture.MongoClient, _publisherMock.Object, _timeProvider);
            _database = fixture.MongoClient.GetDatabase(DbConstants.DbName);
            _database.DropCollection(DbConstants.UrlsTableName);
            MongoDbIndexConfiguration.ConfigureUniqueIndexesAsync(fixture.MongoClient).GetAwaiter().GetResult();
        }

#region ADD_ASYNC_TESTS
        [Fact]
        public async Task AddAsync_Should_InsertUrlSuccessfully_WhenAliasIsUnique()
        {
            // Arrange
            Guid urlId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "short url";
            var url = Url.Create(
                urlId,
                userId,
                longUrl,
                alias,
                _timeProvider.GetUtcNow().AddDays(13),
                _timeProvider
            ).Value;

            // Act
            var result = await _urlRepository.AddAsync(url);

            // Assert
            result.Should().BeTrue();
            var collection = _database.GetCollection<Url>(DbConstants.UrlsTableName);
            var foundInDb = await collection.Find(u => u.Id == url.Id).FirstOrDefaultAsync();
            foundInDb.Should().NotBeNull();
            foundInDb.LongUrl.Should().Be(longUrl);
        }

        [Fact]
        public async Task AddAsync_Should_ThrowException_WhenEmailIsNotUnique()
        {
            // Arrange
            Guid urlId1 = Guid.NewGuid();
            Guid userId1 = Guid.NewGuid();
            const string longUrl1 = "long url1";
            const string alias1 = "short url";
            var url1 = Url.Create(
                urlId1,
                userId1,
                longUrl1,
                alias1,
                _timeProvider.GetUtcNow().AddDays(13),
                _timeProvider
            ).Value;

            Guid urlId2 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            const string longUrl2 = "long url2";
            const string alias2 = "short url";
            var url2 = Url.Create(
                urlId2,
                userId2,
                longUrl2,
                alias2,
                _timeProvider.GetUtcNow().AddDays(13),
                _timeProvider
            ).Value;

            // Act
            var result1 = await _urlRepository.AddAsync(url1);
            var result2 = await _urlRepository.AddAsync(url2);

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeFalse();
            var collection = _database.GetCollection<Url>(DbConstants.UrlsTableName);
            var foundInDb = await collection.Find(u => u.LongUrl == url1.LongUrl).ToListAsync();
            foundInDb.Should().NotBeNull();
            foundInDb.Should().ContainSingle()
                .Which.Id.Should().Be(urlId1);
        }
#endregion // ADD_ASYNC_TESTS

#region GET_URL_BY_ID_ASYNC
        [Fact]
        public async Task GetUrlByIdAsync_Should_ReturnUrl_WhenIdIsCorrect()
        {
            // Arrange
            Guid urlId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "short url";
            var url = Url.Create(
                urlId,
                userId,
                longUrl,
                alias,
                _timeProvider.GetUtcNow().AddDays(13),
                _timeProvider
            ).Value;
            await _urlRepository.AddAsync(url);

            // Act
            var userResult = await _urlRepository.GetUrlByIdAsync(urlId);

            // Assert
            userResult.Should().NotBeNull();
            userResult.Id.Should().Be(urlId);
        }

        [Fact]
        public async Task GetUrlByIdAsync_Should_ReturnNull_WhenIdIsNotFound()
        {
            // Arrange
            Guid urlId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "short url";
            var url = Url.Create(
                urlId,
                userId,
                longUrl,
                alias,
                _timeProvider.GetUtcNow().AddDays(13),
                _timeProvider
            ).Value;
            await _urlRepository.AddAsync(url);

            // Act
            var userResult = await _urlRepository.GetUrlByIdAsync(Guid.NewGuid());

            // Assert
            userResult.Should().BeNull();
        }
#endregion // GET_URL_BY_ID_TESTS

#region GET_URL_BY_ALIAS_ASYNC
        [Fact]
        public async Task GetUrlByAliasAsync_Should_ReturnUrl_WhenAliasIsCorrect()
        {
            // Arrange
            Guid urlId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "short url";
            var url = Url.Create(
                urlId,
                userId,
                longUrl,
                alias,
                _timeProvider.GetUtcNow().AddDays(13),
                _timeProvider
            ).Value;
            await _urlRepository.AddAsync(url);

            // Act
            var userResult = await _urlRepository.GetUrlByAliasAsync(alias);

            // Assert
            userResult.Should().NotBeNull();
            userResult.Id.Should().Be(urlId);
        }

        [Fact]
        public async Task GetUrlByAliasAsync_Should_ReturnNull_WhenAliasIsNotFound()
        {
            // Arrange
            Guid urlId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            const string longUrl = "long url";
            const string alias = "short url";
            var url = Url.Create(
                urlId,
                userId,
                longUrl,
                alias,
                _timeProvider.GetUtcNow().AddDays(13),
                _timeProvider
            ).Value;
            await _urlRepository.AddAsync(url);

            // Act
            var userResult = await _urlRepository.GetUrlByAliasAsync(Guid.NewGuid().ToString());

            // Assert
            userResult.Should().BeNull();
        }
#endregion // GET_URL_BY_ALIAS_TESTS
    }
}
