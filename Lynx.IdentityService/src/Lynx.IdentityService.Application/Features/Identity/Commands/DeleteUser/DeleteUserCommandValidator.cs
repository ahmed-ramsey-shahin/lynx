using FluentValidation;
using Lynx.IdentityService.Application.Common;
using Lynx.IdentityService.Application.Common.Errors;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.DeleteUser
{
    public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
        {
            RuleFor(command => command.Password)
                .Matches(ApplicationRegex.Password)
                .WithErrorCode(ApplicationErrors.PasswordInvalid.Code)
                .WithMessage(ApplicationErrors.PasswordInvalid.Description);

            RuleFor(command => command.HasConfirmed)
                .Equal(true)
                .WithErrorCode(ApplicationErrors.DeletionNotConfirmed.Code)
                .WithMessage(ApplicationErrors.DeletionNotConfirmed.Description);
        }
    }
}
