using Lynx.IdentityService.Application.Common.Interfaces;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.RequestPasswordReset
{
    public sealed record RequestPasswordResetCommand : IRequest<Result<Success>>, IIdempotentCommand
    {
        public string Email { get; init; } = null!;
        public string IdempotencyKey { get; init; } = null!;
    }
}
