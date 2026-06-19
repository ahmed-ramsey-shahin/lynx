using System.Text.Json;
using Lynx.RedirectionService.Application.Common.Services;

namespace Lynx.RedirectionService.Infrastructure.Services.RabbitMq
{
    public sealed class BackgroundMessagePublishingService(IMessageChannel messageChannel) : IMessagePublishingService
    {
        public async Task PublishAsync<TValue>(string queue, TValue body, CancellationToken cancellationToken = default)
        {
            var serialzedBody = JsonSerializer.Serialize(body);
            var message = new QueuedMessage(queue, serialzedBody);
            await messageChannel.QueueMessageAsync(message, cancellationToken);
        }
    }
}
