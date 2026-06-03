using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Common.Settings;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Common.Results;
using Lynx.IdentityService.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.RequestPasswordReset
{
    public sealed class RequestPasswordResetCommandHandler(
        IUserRepository userRepo,
        ILogger<RequestPasswordResetCommandHandler> logger,
        ICacheService cacheService,
        IEmailBackgroundQueue emailQueue,
        IOTPGeneratorService otpService,
        IOptions<ClientUrlOptions> options
    ) : IRequestHandler<RequestPasswordResetCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepo.GetUserByEmailAsync(request.Email, cancellationToken);

            if (user is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("No user was found with {Email}.", request.Email);

                return Result.Success;
            }

            if (!user.IsActivated)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("{UserId} is not activated.", user.Id);
                return UserErrors.NotActivated;
            }

            var otp = otpService.GenerateResetCode();
            await cacheService.SetAsync($"reset_password_otps:{user.Id}", otp, TimeSpan.FromMinutes(15), cancellationToken);
            await emailQueue.QueueEmailAsync(
                new EmailJob(
                    user.Email,
                    user.Username,
                    "Reset Password OTP",
                    $@"You requested a password reset for your account.
                    The OTP for your password reset is: {otp}
                    Please visit {options.Value.ResetPasswordUrl} to create a new password using the OTP given above.
                    Please ignore this email if you did not request a password reset.
                    "
                ),
                cancellationToken
            );

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("OTP sent to {UserName} on {Email}.", user.Username, user.Email);

            return Result.Success;
        }
    }
}
