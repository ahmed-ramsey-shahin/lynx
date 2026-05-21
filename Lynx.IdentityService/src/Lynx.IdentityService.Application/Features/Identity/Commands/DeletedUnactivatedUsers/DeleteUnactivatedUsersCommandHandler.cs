using Lynx.IdentityService.Application.Common.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.DeletedUnactivatedUsers
{
    public class DeleteUnactivatedUsersCommandHandler(
        ILogger<DeleteUnactivatedUsersCommandHandler> logger,
        IUserRepository userRepo,
        TimeProvider timeProvider
    ) : IRequestHandler<DeleteUnactivatedUsersCommand>
    {
        public async Task Handle(DeleteUnactivatedUsersCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting unactivated users from the database...");
            var cutoffDate = timeProvider.GetUtcNow().AddHours(-2);

            try
            {
                var numberOfDeletedUsers = await userRepo.DeleteUnactivatedUsersAsync(cutoffDate, cancellationToken);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Successfully deleted {NumberOfDeletedUsers} expired accounts.", numberOfDeletedUsers);
            }
            catch(Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "A critical error occurred while attempting to bulk delete unactivated users.");

                throw;
            }
        }
    }
}
