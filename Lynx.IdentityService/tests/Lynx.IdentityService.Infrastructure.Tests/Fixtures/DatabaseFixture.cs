using Lynx.IdentityService.Infrastructure.Data.Configuration;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Lynx.IdentityService.Infrastructure.Tests.Fixtures
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:7.0")
            .Build();
        public IMongoClient MongoClient { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _mongoContainer.StartAsync();
            MongoClient = new MongoClient(_mongoContainer.GetConnectionString());
            Data.Configuration.MongoDbConfiguration.ConfigureMappings();
            await MongoDbIndexConfiguration.ConfigureUniqueIndexesAsync(MongoClient);
        }

        public async Task DisposeAsync()
        {
            await _mongoContainer.DisposeAsync();
        }
    }

    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
}
