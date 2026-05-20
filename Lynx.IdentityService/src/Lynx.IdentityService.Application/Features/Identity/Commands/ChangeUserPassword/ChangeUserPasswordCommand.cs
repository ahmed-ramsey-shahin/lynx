using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUserPassword
{
    public sealed record ChangeUserPasswordCommand : IRequest<Result<Updated>>
    {
        public Guid UserId { get; init; }
        public string NewPassword { get; init; } = null!;
        public string OldPassword { get; init; } = null!;
    }
}
