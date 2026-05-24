using System.Security.Claims;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace Lynx.IdentityService.Application.Common.Services
{
    public interface ITokenProvider
    {
        TokenDto? GenerateJwtToken(UserDto user);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        JsonWebKey GetPublicKeyJwk();
    }
}
