using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.ActivateUser
{
    public sealed class ActivateUserCommandHandler(
        IUserRepository userRepo,
        ICacheService cacheService,
        ILogger<ActivateUserCommandHandler> logger,
        TimeProvider timeProvider
    ) : IRequestHandler<ActivateUserCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            var cacheKey = $"activation-codes:{request.ActivationCode}";
            var userId = await cacheService.GetAsync<Guid?>(cacheKey, cancellationToken);

            if (userId is null)
            {
                logger.LogWarning("Could not activate the user. The activation code was not found.");
                return ApplicationErrors.ActivationCodeExpired;
            }

            var user = (await userRepo.GetUserByIdAsync(userId.Value, cancellationToken))!;
            var activationResult = user.Activate(timeProvider.GetUtcNow());

            if (activationResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Could not activate user {UserId}. {@Errors}", userId.Value, activationResult.Errors);

                return activationResult.Errors!;
            }

            await userRepo.UpdateAsync(user, cancellationToken);
            await cacheService.RemoveAsync(cacheKey, cancellationToken);
            return Result.Updated;
        }
    }
}
