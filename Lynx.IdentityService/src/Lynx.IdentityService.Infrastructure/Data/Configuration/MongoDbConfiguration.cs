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
                    cm.MapField("_refreshTokens").SetElementName("RefreshTokens");
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(RefreshToken)))
            {
                BsonClassMap.RegisterClassMap<RefreshToken>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(rt => rt.Token);
                    cm.MapProperty(rt => rt.ExpiresOn);
                    cm.MapProperty(rt => rt.IsRevoked);
                });
            }
        }
    }
}
