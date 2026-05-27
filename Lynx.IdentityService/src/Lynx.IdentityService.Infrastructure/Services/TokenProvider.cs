using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lynx.IdentityService.Infrastructure.Services
{
    public class TokenProvider(
        IOptions<JwtSettings> options,
        TimeProvider timeProvider,
        IOTPGeneratorService OtpGenerator
    ) : ITokenProvider
    {
        public TokenDto? GenerateJwtToken(UserDto user)
        {
            var expirationDate = timeProvider.GetUtcNow().AddMinutes(options.Value.ExpiryMinutes).UtcDateTime;
            using var rsa = RSA.Create();
            rsa.ImportFromPem(options.Value.PrivateKey);
            var securityKey = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
            List<Claim> claims = [
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Username)
            ];
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expirationDate,
                Issuer = options.Value.Issuer,
                Audience = options.Value.Audience,
                SigningCredentials = credentials,
                IssuedAt = timeProvider.GetUtcNow().UtcDateTime,
                NotBefore = timeProvider.GetUtcNow().UtcDateTime
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            var refreshToken = OtpGenerator.GenerateUrlSafeToken(64);
            return new TokenDto
            {
                AccessToken = token,
                ExpiresAt = expirationDate,
                RefreshToken = refreshToken
            };
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(options.Value.PrivateKey);
            var securityKey = new RsaSecurityKey(rsa);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = options.Value.Audience,
                ValidIssuer = options.Value.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (
                securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.OrdinalIgnoreCase)
            )
            {
                throw new SecurityTokenException("Invalid token algorithm");
            }

            return principal;
        }

        public JsonWebKey GetPublicKeyJwk()
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(options.Value.PrivateKey);
            var securityKey = new RsaSecurityKey(rsa)
            {
                KeyId = "lynx-auth-key-1"
            };
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(securityKey);
            jwk.Alg = SecurityAlgorithms.RsaSha256;
            jwk.Use = "sig";
            return jwk;
        }
    }
}
