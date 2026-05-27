using Lynx.IdentityService.Contracts;

namespace Lynx.IdentityService.Application.Common.BackgroundJobs
{
    public interface IEmailBackgroundQueue
    {
        ValueTask QueueEmailAsync(EmailJob job, CancellationToken cancellationToken=default);
        IAsyncEnumerable<EmailJob> DequeueAllAsync(CancellationToken cancellationToken=default);
    }
}
