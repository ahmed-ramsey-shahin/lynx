using FluentValidation;
using Lynx.IdentityService.Application.Common;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.RequestPasswordReset
{
    public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
    {
        public RequestPasswordResetCommandValidator()
        {
            RuleFor(command => command.Email)
                .Matches(ApplicationRegex.Email)
                .WithErrorCode(ApplicationErrors.EmailInvalid.Code)
                .WithMessage(ApplicationErrors.EmailInvalid.Description);
        }
    }
}
