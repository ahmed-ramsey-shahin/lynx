using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Infrastructure.Data;
using Lynx.IdentityService.Infrastructure.Data.Configuration;
using Lynx.IdentityService.Infrastructure.Exceptions;
using Lynx.IdentityService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Lynx.IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        public static async Task ConfigureMongoDbAsync(IServiceProvider serviceProvider)
        {
            MongoDbConfiguration.ConfigureMappings();
            await MongoDbIndexConfiguration.ConfigureUniqueIndexesAsync(serviceProvider.GetService<IMongoClient>()!);
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }

        private static IServiceCollection AddRedisCache(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }

        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration config)
        {
            var mongoDbConnectionString = config.GetConnectionString("MongoDbConnectionString") ?? throw new InfrastructureConfigurationException("MongoDbConnectionString");
            var redisConnectionString = config.GetConnectionString("RedisConnectionString") ?? throw new InfrastructureConfigurationException("RedisConnectionString");
            services.AddMongoDb(mongoDbConnectionString)
                .AddRedisCache(redisConnectionString);
            return services;
        }
    }
}
