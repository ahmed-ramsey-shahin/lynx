using FluentValidation;
using Lynx.IdentityService.Application.Common;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.GenerateToken
{
    public sealed class GenerateTokenCommandValidator : AbstractValidator<GenerateTokenCommand>
    {
        public GenerateTokenCommandValidator()
        {
            RuleFor(query => query.Username)
                .Matches(ApplicationRegex.Username)
                .WithErrorCode(ApplicationErrors.UsernameInvalid.Code)
                .WithMessage(ApplicationErrors.UsernameInvalid.Description);

            RuleFor(query => query.Password)
                .Matches(ApplicationRegex.Password)
                .WithErrorCode(ApplicationErrors.PasswordInvalid.Code)
                .WithMessage(ApplicationErrors.PasswordInvalid.Description);
        }
    }
}
