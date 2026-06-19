using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Infrastructure.Exceptions;
using Lynx.RedirectionService.Infrastructure.BackgroundServices;
using Lynx.RedirectionService.Infrastructure.Services.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lynx.RedirectionService.Infrastructure
{
    public static class DependencyInjection
    {
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
            var rabbitMqConnectionString = config.GetConnectionString("RabbitMQ") ?? throw new InfrastructureConfigurationException("ConnectionStrings:RabbitMQ");
            services.AddRabbitMQ(rabbitMqConnectionString);
            return services;
        }
    }
}
