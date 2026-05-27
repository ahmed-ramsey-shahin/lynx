using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Identity;
using MediatR;
namespace Lynx.IdentityService.Application.Features.Identity.EventHandlers
{
    public class UserActivatedEventHandler(IEmailBackgroundQueue emailQueue) : INotificationHandler<UserActivatedEvent>
    {
        public async Task Handle(UserActivatedEvent notification, CancellationToken cancellationToken)
        {
            await emailQueue.QueueEmailAsync(
                new EmailJob(
                    notification.Email,
                    notification.Username,
                    "Account activation",
                    $"Hi {notification.Username}, you account has been successfully activated."
                ),
                cancellationToken
            );
        }
    }
}
