using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Queries.GenerateToken
{
    public sealed record GenerateTokenQuery : IRequest<Result<TokenDto>>
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
