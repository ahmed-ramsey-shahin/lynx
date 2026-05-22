using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUsername
{
    public sealed record ChangeUsernameCommand : IRequest<Result<Updated>>
    {
        public Guid UserId { get; init; }
        public string NewUsername { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
