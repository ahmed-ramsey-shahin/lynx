using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Identity;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.EventHandlers
{
    public class UserDeletedEventHandler(IEmailBackgroundQueue emailQueue) : INotificationHandler<UserDeletedEvent>
    {
        public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
        {
            await emailQueue.QueueEmailAsync(
                new EmailJob(
                    notification.Email,
                    notification.Username,
                    "Account Deleted",
                    $"Hi {notification.Username}, your account has been deleted. We are sorry you had to leave us."
                ),
                cancellationToken
            );
        }
    }
}
