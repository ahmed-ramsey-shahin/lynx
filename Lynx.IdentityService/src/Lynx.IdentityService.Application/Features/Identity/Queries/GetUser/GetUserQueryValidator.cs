using FluentValidation;
using Lynx.IdentityService.Application.Common;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Queries.GetUser
{
    public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
    {
        public GetUserQueryValidator()
        {
            RuleFor(command => command.Username)
                .Matches(ApplicationRegex.Username)
                .WithErrorCode(ApplicationErrors.UsernameInvalid.Code)
                .WithMessage(ApplicationErrors.UsernameInvalid.Description);
        }
    }
}
