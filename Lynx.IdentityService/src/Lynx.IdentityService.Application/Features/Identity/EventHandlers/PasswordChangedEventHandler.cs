using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Identity;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.EventHandlers
{
    public class PasswordChangedEventHandler(IEmailBackgroundQueue emailQueue) : INotificationHandler<PasswordChangedEvent>
    {
        public async Task Handle(PasswordChangedEvent notification, CancellationToken cancellationToken)
        {
            await emailQueue.QueueEmailAsync(
                new EmailJob(
                    notification.Username,
                    notification.Email,
                    "Your password has changed",
                    @$"Hi {notification.Username} your password has changed.
                    If you did not attempt to change it, please contact the customer support ASAP."
                ),
                cancellationToken
            );
        }
    }
}
