using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand : IRequest<Result<TokenDto>>
    {
        public string RefreshToken { get; init; } = default!;
        public string ExpiredAccessToken { get; init; } = default!;
    }
}
