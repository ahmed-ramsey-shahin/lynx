using Lynx.IdentityService.Domain.Identity;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Lynx.IdentityService.Infrastructure.Data.Configuration
{
    public static class MongoDbConfiguration
    {
        public static void ConfigureMappings()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
            {
                BsonClassMap.RegisterClassMap<User>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(user => user.Id).SetIdGenerator(GuidGenerator.Instance);
                });
            }
        }
    }
}
