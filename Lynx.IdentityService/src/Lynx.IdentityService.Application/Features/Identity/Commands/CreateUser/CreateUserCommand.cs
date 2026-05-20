using Lynx.IdentityService.Application.Common.Interfaces;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser
{
    public sealed record CreateUserCommand : IRequest<Result<Guid>>, IIdempotentCommand
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string IdempotencyKey { get; init; } = null!;
    }
}
