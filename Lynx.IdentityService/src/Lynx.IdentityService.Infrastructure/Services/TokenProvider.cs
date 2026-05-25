using System.Security.Claims;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace Lynx.IdentityService.Infrastructure.Services
{
    public class TokenProvider : ITokenProvider
    {
        public TokenDto? GenerateJwtToken(UserDto user)
        {
            throw new NotImplementedException();
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            throw new NotImplementedException();
        }

        public JsonWebKey GetPublicKeyJwk()
        {
            throw new NotImplementedException();
        }
    }
}
