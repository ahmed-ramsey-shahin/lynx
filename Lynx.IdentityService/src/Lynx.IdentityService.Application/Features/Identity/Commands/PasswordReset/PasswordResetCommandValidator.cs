using FluentValidation;
using Lynx.IdentityService.Application.Common;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.PasswordReset
{
    public sealed class PasswordResetCommandValidator : AbstractValidator<PasswordResetCommand>
    {
        public PasswordResetCommandValidator()
        {
            RuleFor(command => command.Email)
                .Matches(ApplicationRegex.Email)
                .WithErrorCode(ApplicationErrors.EmailInvalid.Code)
                .WithMessage(ApplicationErrors.EmailInvalid.Description);

            RuleFor(command => command.NewPassword)
                .Matches(ApplicationRegex.Password)
                .WithErrorCode(ApplicationErrors.PasswordInvalid.Code)
                .WithMessage(ApplicationErrors.PasswordInvalid.Description);

            RuleFor(command => command.Code)
                .Matches("^[0-9]{6}$")
                .WithErrorCode(ApplicationErrors.OtpInvalid.Code)
                .WithMessage(ApplicationErrors.OtpInvalid.Description);
        }
    }
}
