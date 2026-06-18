namespace Lynx.RedirectionService.Application.Common.Services
{
    public interface IMessagePublishingService
    {
        Task PublishAsync<TValue>(string queue, TValue body, CancellationToken cancellationToken=default);
    }
}
