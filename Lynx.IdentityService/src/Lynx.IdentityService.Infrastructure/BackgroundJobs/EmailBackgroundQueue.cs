using System.Threading.Channels;
using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Contracts;

namespace Lynx.IdentityService.Infrastructure.BackgroundJobs
{
    public class EmailBackgroundQueue : IEmailBackgroundQueue
    {
        private readonly Channel<EmailJob> _channel = Channel.CreateUnbounded<EmailJob>();

        public IAsyncEnumerable<EmailJob> DequeueAllAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }

        public async ValueTask QueueEmailAsync(EmailJob job, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(job, cancellationToken);
        }
    }
}
