using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.GenerateToken
{
    public sealed record GenerateTokenCommand : IRequest<Result<TokenDto>>
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
