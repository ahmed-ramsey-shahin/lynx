namespace Lynx.IdentityService.Application.Common.Services
{
    public interface IMessagePublishingService
    {
        Task PublishAsync<TValue>(string queue, TValue body);
    }
}
