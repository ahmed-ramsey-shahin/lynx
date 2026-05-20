using Lynx.IdentityService.Application.Common.Interfaces;
using Lynx.IdentityService.Domain.Common.Results;
using Microsoft.IdentityModel.Tokens;

namespace Lynx.IdentityService.Application.Features.Identity.Queries.GetJwks
{
    public sealed record GetJwksQuery : ICachedQuery<Result<JsonWebKeySet>>
    {
        public string CacheKey => "jwks";

        public TimeSpan Expiration => TimeSpan.FromHours(24);
    }
}
