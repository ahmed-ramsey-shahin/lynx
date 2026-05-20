using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.PasswordReset
{
    public sealed class PasswordResetCommandHandler(
        IPasswordHashingService hashingService,
        IUserRepository userRepo,
        ICacheService cacheService,
        ILogger<PasswordResetCommandHandler> logger
    ) : IRequestHandler<PasswordResetCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(PasswordResetCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepo.GetUserByEmailAsync(request.Email, cancellationToken);

            if (user is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("No user was found with {Email}.", request.Email);

                return ApplicationErrors.OtpExpired;
            }

            var cacheKey = $"reset_password_otps:{user.Username}";
            var cachedOtp = await cacheService.GetAsync<string>(cacheKey, cancellationToken);

            if (cachedOtp?.Equals(request.Code) != true)
            {
                logger.LogWarning("OTP Invalid or Expired.");
                return ApplicationErrors.OtpExpired;
            }

            var hashedPassword = hashingService.Hash(request.NewPassword);
            var changePasswordResult = user.ChangePassword(hashedPassword);

            if (changePasswordResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Could not change user's password. {@Errors}.", changePasswordResult.Errors);

                return changePasswordResult.Errors!;
            }

            await userRepo.UpdateAsync(user, cancellationToken);
            await cacheService.RemoveAsync(cacheKey, cancellationToken);
            logger.LogInformation("Password changed successfully.");
            return Result.Updated;
        }
    }
}
