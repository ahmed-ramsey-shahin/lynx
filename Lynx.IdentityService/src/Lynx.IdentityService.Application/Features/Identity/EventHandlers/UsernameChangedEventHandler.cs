using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Identity;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.EventHandlers
{
    public class UsernameChangedEventHandler(IEmailBackgroundQueue emailQueue) : INotificationHandler<UsernameChangedEvent>
    {
        public async Task Handle(UsernameChangedEvent notification, CancellationToken cancellationToken)
        {
            await emailQueue.QueueEmailAsync(
                new EmailJob(
                    notification.Email,
                    notification.NewUsername,
                    "Your username has changed",
                    @$"Hi {notification.NewUsername}, your username has changed from {notification.OldUsername} to {notification.NewUsername}.
If you did not attempt to change it, please contact the customer support ASAP."
                ),
                cancellationToken
            );
        }
    }
}
