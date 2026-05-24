using Lynx.IdentityService.Domain.Identity;
using MongoDB.Driver;

namespace Lynx.IdentityService.Infrastructure.Data.Configuration
{
    public static class MongoDbIndexConfiguration
    {
        public static async Task ConfigureUniqueIndexesAsync(IMongoClient client)
        {
            var database = client.GetDatabase(DbConstants.DbName);
            var usersCollection = database.GetCollection<User>(DbConstants.UserTableName);

            var usernameIndexKey = Builders<User>.IndexKeys.Ascending(user => user.Username);
            var usernameIndexOptions = new CreateIndexOptions
            {
                Unique = true,
                Name = DbConstants.UsernameIndexName
            };
            var usernameIndexModel = new CreateIndexModel<User>(usernameIndexKey, usernameIndexOptions);

            var emailIndexKey = Builders<User>.IndexKeys.Ascending(user => user.Email);
            var emailIndexOptions = new CreateIndexOptions
            {
                Unique = true,
                Name = DbConstants.EmailIndexName
            };
            var emailIndexModel = new CreateIndexModel<User>(emailIndexKey, emailIndexOptions);

            await usersCollection.Indexes.CreateManyAsync(
            [
                usernameIndexModel,
                emailIndexModel
            ]);
        }
    }
}
