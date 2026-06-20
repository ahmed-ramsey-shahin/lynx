using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Domain.Urls;
using MediatR;
using MongoDB.Driver;

namespace Lynx.RedirectionService.Infrastructure.Data
{
    public class UrlRepository : IUrlRepository
    {
        private readonly IMongoCollection<Url> _urls;
        private readonly IMongoClient _client;
        private readonly IPublisher _publisher;
        private readonly TimeProvider _timeProvider;

        public UrlRepository(IMongoClient client, IPublisher publisher, TimeProvider timeProvider)
        {
            _client = client;
            var database = _client.GetDatabase(DbConstants.DbName);
            _urls = database.GetCollection<Url>(DbConstants.UrlsTableName);
            _publisher = publisher;
            _timeProvider = timeProvider;
        }

        public async Task<bool> AddAsync(Url url, CancellationToken cancellationToken = default)
        {
            url.CreatedAt = _timeProvider.GetUtcNow();

            try
            {
                await _urls.InsertOneAsync(url, null, cancellationToken);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }

            foreach (var evt in url.Events)
            {
                await _publisher.Publish(evt, cancellationToken);
            }

            url.ClearEvents();
            return true;
        }

        public async Task<bool> AliasExistsAsync(string alias, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Url>.Filter.Eq(url => url.Alias, alias);
            return await _urls.Find(filter).AnyAsync(cancellationToken);
        }

        public async Task<Url?> GetUrlByAliasAsync(string alias, CancellationToken cancellationToken = default)
        {
            return await _urls.Find(url => url.Alias == alias).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Url?> GetUrlByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _urls.Find(url => url.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> UpdateUrlAsync(Url url, CancellationToken cancellationToken = default)
        {
            url.UpdatedAt = _timeProvider.GetUtcNow();
            var result = await _urls.ReplaceOneAsync(
                u => u.Id == url.Id,
                url,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken
            );

            if (!result.IsAcknowledged || result.MatchedCount <= 0)
            {
                return false;
            }

            foreach (var evt in url.Events)
            {
                await _publisher.Publish(evt, cancellationToken);
            }

            url.ClearEvents();
            return true;
        }
    }
}
