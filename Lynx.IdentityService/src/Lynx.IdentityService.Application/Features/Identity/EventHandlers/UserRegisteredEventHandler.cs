using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Identity;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.EventHandlers
{
    public class UserRegisteredEventHandler(IEmailBackgroundQueue emailQueue) : INotificationHandler<UserRegisteredEvent>
    {
        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            await emailQueue.QueueEmailAsync(
                new EmailJob(
                    notification.Email,
                    notification.Username,
                    "Welcome to Lynx",
                    $"Hi {notification.Username}, welcome to Lynx."
                ),
                cancellationToken
            );
        }
    }
}
