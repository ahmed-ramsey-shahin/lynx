using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.PasswordReset
{
    public sealed record PasswordResetCommand : IRequest<Result<Updated>>
    {
        public string Email { get; init; } = null!;
        public string Code { get; init; } = null!;
        public string NewPassword { get; init; } = null!;
    }
}
