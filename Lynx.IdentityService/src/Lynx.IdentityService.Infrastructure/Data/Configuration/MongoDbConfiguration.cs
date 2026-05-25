using Lynx.IdentityService.Domain.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace Lynx.IdentityService.Infrastructure.Data.Configuration
{
    public static class MongoDbConfiguration
    {
        public static void ConfigureMappings()
        {
            try
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
            catch (BsonSerializationException)
            {}

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
