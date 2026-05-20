using FluentValidation;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser
{
    public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(command => command.Email)
                .Matches("""(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])""")
                .WithErrorCode(ApplicationErrors.EmailInvalid.Code)
                .WithMessage(ApplicationErrors.EmailInvalid.Description);

            RuleFor(command => command.Username)
                .Matches("/^[a-zA-Z][a-zA-Z0-9_]{2,15}$/")
                .WithErrorCode(ApplicationErrors.UsernameInvalid.Code)
                .WithMessage(ApplicationErrors.UsernameInvalid.Description);

            RuleFor(command => command.Password)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithErrorCode(ApplicationErrors.PasswordInvalid.Code)
                .WithMessage(ApplicationErrors.PasswordInvalid.Description);
        }
    }
}
