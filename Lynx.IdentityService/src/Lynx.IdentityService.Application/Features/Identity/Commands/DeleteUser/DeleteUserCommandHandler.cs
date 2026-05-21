using Lynx.IdentityService.Application.Common;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.DeleteUser
{
    public sealed class DeleteUserCommandHandler(
        IPasswordHashingService hashingService,
        ILogger<DeleteUserCommandHandler> logger,
        IUserRepository userRepo,
        IMessagePublishingService publishingService,
        ICacheService cacheService
    ) : IRequestHandler<DeleteUserCommand, Result<Deleted>>
    {
        public async Task<Result<Deleted>> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!request.HasConfirmed)
            {
                logger.LogInformation("User deletion stopped. The user did not agree to delete all their data.");
                return ApplicationErrors.DeletionNotConfirmed;
            }

            var user = await userRepo.GetUserByIdAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("No user was found with {UserId}.", request.UserId);

                return ApplicationErrors.UserNotFound;
            }

            if (!hashingService.Verify(request.Password, user.Password))
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Request could not be processed. The provided password is incorrect.");

                return ApplicationErrors.InvalidOldPassword;
            }

            var deleteResult = user.Delete();

            if (deleteResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Could not delete {Username}. {@Errors}.", user.Username, deleteResult.Errors);
                return deleteResult.Errors!;
            }

            await userRepo.UpdateAsync(user, cancellationToken);
            await publishingService.PublishAsync(PublishingQueues.DeleteUser, user.Id);
            await cacheService.RemoveAsync($"users:{user.Username}", cancellationToken);
            return Result.Deleted;
        }
    }
}
