using FluentValidation;
using Lynx.IdentityService.Application.Common;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUsername
{
    public class ChangeUsernameCommandValidator : AbstractValidator<ChangeUsernameCommand>
    {
        public ChangeUsernameCommandValidator()
        {
            RuleFor(command => command.Password)
                .Matches(ApplicationRegex.Password)
                .WithErrorCode(ApplicationErrors.PasswordInvalid.Code)
                .WithMessage(ApplicationErrors.PasswordInvalid.Description);

            RuleFor(command => command.Username)
                .Matches(ApplicationRegex.Username)
                .WithErrorCode(ApplicationErrors.UsernameInvalid.Code)
                .WithMessage(ApplicationErrors.UsernameInvalid.Description);
        }
    }
}
