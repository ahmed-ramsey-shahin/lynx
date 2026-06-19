using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Infrastructure.Services.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lynx.RedirectionService.Infrastructure.BackgroundServices
{
    public sealed class RabbitMqPublisherBackgroundService(
        IMessageChannel messageChannel,
        ILogger<RabbitMqPublisherBackgroundService> logger,
        [FromKeyedServices("instant")] IMessagePublishingService messagePublishingService
    ) : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("RabbitMQ background publisher is starting.");

            await foreach(var message in messageChannel.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await messagePublishingService.PublishAsync(message.QueueName, message.SerializedBody, stoppingToken);
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Successfully published background message to {Queue}.", message.QueueName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to publish message to RabbitMQ.");
                }
            }
        }
    }
}
