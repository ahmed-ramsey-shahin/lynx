using System.Text.Json;
using Lynx.IdentityService.Application.Common.Services;
using RabbitMQ.Client;

namespace Lynx.IdentityService.Infrastructure.Services.RabbitMq
{
    public class MessagePublishingService(
        IRabbitMqChannelPool channelPool
    ) : IMessagePublishingService
    {
        private async Task PublishAsync(string queue, byte[] bytes, string contentType, CancellationToken cancellationToken)
        {
            var channel = await channelPool.GetChannelAsync(cancellationToken);
            var props = new BasicProperties()
            {
                DeliveryMode = DeliveryModes.Persistent,
                ContentType = contentType,
                AppId = "IdentityService"
            };

            try
            {
                await channel.BasicPublishAsync(
                    string.Empty,
                    queue,
                    false,
                    props,
                    bytes,
                    cancellationToken
                );
            }
            finally
            {
                channelPool.ReturnChannel(channel);
            }
        }

        public async Task PublishAsync(string queue, Guid body, CancellationToken cancellationToken=default)
        {
            var message = body.ToByteArray();
            await PublishAsync(queue, message, "application/octet-stream", cancellationToken);
        }

        public async Task PublishAsync<TValue>(string queue, TValue body, CancellationToken cancellationToken=default)
        {
            var message = JsonSerializer.SerializeToUtf8Bytes(body);
            await PublishAsync(queue, message, "application/json", cancellationToken);
        }
    }
}
