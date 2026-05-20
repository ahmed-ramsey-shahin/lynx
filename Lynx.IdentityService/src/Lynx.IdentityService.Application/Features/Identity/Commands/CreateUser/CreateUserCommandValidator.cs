using FluentValidation;
using Lynx.IdentityService.Application.Common;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser
{
    public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(command => command.Email)
                .Matches(ApplicationRegex.Email)
                .WithErrorCode(ApplicationErrors.EmailInvalid.Code)
                .WithMessage(ApplicationErrors.EmailInvalid.Description);

            RuleFor(command => command.Username)
                .Matches(ApplicationRegex.Username)
                .WithErrorCode(ApplicationErrors.UsernameInvalid.Code)
                .WithMessage(ApplicationErrors.UsernameInvalid.Description);

            RuleFor(command => command.Password)
                .Matches(ApplicationRegex.Password)
                .WithErrorCode(ApplicationErrors.PasswordInvalid.Code)
                .WithMessage(ApplicationErrors.PasswordInvalid.Description);
        }
    }
}
