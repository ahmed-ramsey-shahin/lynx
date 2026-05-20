using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.RequestPasswordReset
{
    public class RequestPasswordResetCommandHandler(
        IUserRepository userRepo,
        ILogger<RequestPasswordResetCommandHandler> logger,
        ICacheService cacheService,
        IEmailService emailService,
        IOTPGeneratorService otpService,
        IConfiguration config
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

            var otp = otpService.GenerateResetCode();
            var cacheWriteTask = cacheService.SetAsync($"reset_password_otps:{user.Username}", otp, TimeSpan.FromMinutes(15), cancellationToken);
            var emailTask = emailService.SendEmailAsync(
                user.Email,
                user.Username,
                "Reset Password OTP",
                "You requested a password reset for your account.\n" +
                "The OTP for your password reset is: " + otp + "\n" +
                "Please visit " + config["ResetPasswordUrl"] + " to create a new password using the OTP given above." +
                "Please ignore this email if you did not request a password reset.",
                cancellationToken
            );

            await Task.WhenAll(cacheWriteTask, emailTask);
            return Result.Success;
        }
    }
}
