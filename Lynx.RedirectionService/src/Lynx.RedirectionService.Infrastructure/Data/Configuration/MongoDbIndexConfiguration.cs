using Lynx.RedirectionService.Domain.Urls;
using MongoDB.Driver;

namespace Lynx.RedirectionService.Infrastructure.Data.Configuration
{
    public static class MongoDbIndexConfiguration
    {
        public static async Task ConfigureUniqueIndexesAsync(IMongoClient client)
        {
            var database = client.GetDatabase(DbConstants.DbName);
            var urlsCollection = database.GetCollection<Url>(DbConstants.UrlsTableName);

            var aliasIndexKey = Builders<Url>.IndexKeys.Ascending(url => url.Alias);
            var aliasIndexOptions = new CreateIndexOptions
            {
                Unique = true,
                Name = DbConstants.UrlsAliasIndexName
            };
            var usernameIndexModel = new CreateIndexModel<Url>(aliasIndexKey, aliasIndexOptions);

            await urlsCollection.Indexes.CreateManyAsync(
            [
                usernameIndexModel,
            ]);
        }
    }
}
