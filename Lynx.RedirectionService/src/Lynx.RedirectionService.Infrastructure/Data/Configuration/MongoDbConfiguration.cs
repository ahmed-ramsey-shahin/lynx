using Lynx.RedirectionService.Domain.Urls;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace Lynx.RedirectionService.Infrastructure.Data.Configuration
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

            if (!BsonClassMap.IsClassMapRegistered(typeof(Url)))
            {
                BsonClassMap.RegisterClassMap<Url>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(url => url.Id).SetIdGenerator(GuidGenerator.Instance);
                });
            }
        }
    }
}
