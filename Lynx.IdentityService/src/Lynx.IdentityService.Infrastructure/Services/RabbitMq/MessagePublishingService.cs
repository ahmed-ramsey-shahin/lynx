using Lynx.IdentityService.Application.Common.Services;

namespace Lynx.IdentityService.Infrastructure.Services.RabbitMq
{
    public class MessagePublishingService(
        IRabbitMqChannelPool channelPool
    ) : IMessagePublishingService
    {
        public async Task PublishAsync<TValue>(string queue, TValue body)
        {
            //
        }
    }
}
