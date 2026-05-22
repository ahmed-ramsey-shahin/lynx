using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUserPassword
{
    public sealed class ChangeUserPasswordCommandHandler(
        ILogger<ChangeUserPasswordCommandHandler> logger,
        IUserRepository userRepo,
        IPasswordHashingService hashingService,
        IUserService userService
    ) : IRequestHandler<ChangeUserPasswordCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepo.GetUserByIdAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("No user was found with {UserId}.", request.UserId);

                return ApplicationErrors.UserNotFound;
            }

            if (user.Id != userService.UserId)
            {
                logger.LogWarning("Could not complete request the authenticated user");
                return ApplicationErrors.UserNotOwned;
            }

            if (!hashingService.Verify(request.OldPassword, user.Password))
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Request could not be processed. The provided old password is incorrect.");

                return ApplicationErrors.InvalidOldPassword;
            }

            var passwordHash = hashingService.Hash(request.NewPassword);
            var changePasswordResult = user.ChangePassword(passwordHash);

            if (changePasswordResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Request could not be processed. {@Errors}.", changePasswordResult.Errors);

                return changePasswordResult.Errors!;
            }

            await userRepo.UpdateAsync(user, cancellationToken);
            return Result.Updated;
        }
    }
}
