using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Infrastructure.Exceptions;
using Lynx.RedirectionService.Infrastructure.BackgroundServices;
using Lynx.RedirectionService.Infrastructure.Services.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lynx.RedirectionService.Infrastructure.Data.Configuration;
using MongoDB.Driver;
using Lynx.RedirectionService.Infrastructure.Data;
using Lynx.RedirectionService.Application.Common.Repositories;

namespace Lynx.RedirectionService.Infrastructure
{
    public static class DependencyInjection
    {
        public static async Task ConfigureMongoDbAsync(this IServiceProvider services)
        {
            MongoDbConfiguration.ConfigureMappings();
            var client = services.GetRequiredService<IMongoClient>();
            await MongoDbIndexConfiguration.ConfigureUniqueIndexesAsync(client);
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddScoped<IUrlRepository, UrlRepository>();
            return services;
        }
        private static IServiceCollection AddRabbitMQ(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IRabbitMqConnectionManager>(_ => new RabbitMqConnectionManager(connectionString));
            services.AddSingleton<IRabbitMqChannelPool, RabbitMqChannelPool>();
            services.AddSingleton<IMessagePublishingService, MessagePublishingService>();
            services.AddSingleton<IMessageChannel, MessageChannel>();
            services.AddHostedService<RabbitMqPublisherBackgroundService>();
            services.AddKeyedScoped<IMessagePublishingService, BackgroundMessagePublishingService>("background");
            services.AddKeyedScoped<IMessagePublishingService, MessagePublishingService>("instant");
            return services;
        }
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration config)
        {
            var mongoDbConnectionString = config.GetConnectionString("MongoDB") ?? throw new InfrastructureConfigurationException("ConnectionStrings:MongoDB");
            var rabbitMqConnectionString = config.GetConnectionString("RabbitMQ") ?? throw new InfrastructureConfigurationException("ConnectionStrings:RabbitMQ");
            services.AddRabbitMQ(rabbitMqConnectionString)
                .AddMongoDb(mongoDbConnectionString);
            return services;
        }
    }
}
