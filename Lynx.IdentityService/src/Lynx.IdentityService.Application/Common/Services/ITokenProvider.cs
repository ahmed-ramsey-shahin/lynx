using System.Security.Claims;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;
using Microsoft.IdentityModel.Tokens;

namespace Lynx.IdentityService.Application.Common.Services
{
    public interface ITokenProvider
    {
        Task<Result<TokenDto>> GenerateJwtTokenAsync(UserDto user, CancellationToken cancellationToken=default);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        JsonWebKey GetPublicKeyJwk();
    }
}
