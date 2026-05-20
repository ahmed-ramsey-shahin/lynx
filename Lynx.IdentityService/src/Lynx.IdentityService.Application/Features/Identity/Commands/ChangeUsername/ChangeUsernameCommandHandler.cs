using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUsername
{
    public sealed class ChangeUsernameCommandHandler(
        ILogger<ChangeUsernameCommandHandler> logger,
        IUserRepository userRepo,
        IPasswordHashingService hashingService
    ) : IRequestHandler<ChangeUsernameCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
        {
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

            if (string.Equals(request.Username, user.Username))
            {
                return Result.Updated;
            }

            if (!await userRepo.IsUsernameUniqueAsync(request.Username, cancellationToken))
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Request could not be processed {Username} is not unique.", request.Username);
                return ApplicationErrors.UsernameAlreadyExists;
            }

            var changeUsernameResult = user.ChangeUsername(request.Username);

            if (changeUsernameResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Request could not be processed. {@Errors}.", changeUsernameResult.Errors);

                return changeUsernameResult.Errors!;
            }

            await userRepo.UpdateAsync(user, cancellationToken);
            return Result.Updated;
        }
    }
}
