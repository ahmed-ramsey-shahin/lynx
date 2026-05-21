using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.DeleteUser
{
    public sealed record DeleteUserCommand : IRequest<Result<Deleted>>
    {
        public Guid UserId { get; init; }
        public string Password  { get; init; } = null!;
        public bool HasConfirmed { get; init; }
    }
}
