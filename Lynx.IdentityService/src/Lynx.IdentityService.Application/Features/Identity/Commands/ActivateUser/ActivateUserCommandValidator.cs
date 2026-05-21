using FluentValidation;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.ActivateUser
{
    public sealed class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
    {
        public ActivateUserCommandValidator()
        {
            RuleFor(command => command.ActivationCode)
                .Matches("^[a-zA-Z0-9]{64}$")
                .WithErrorCode(ApplicationErrors.ActivationCodeInvalid.Code)
                .WithMessage(ApplicationErrors.ActivationCodeInvalid.Description);
        }
    }
}
