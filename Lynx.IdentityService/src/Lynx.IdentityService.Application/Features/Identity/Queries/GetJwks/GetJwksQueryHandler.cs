using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace Lynx.IdentityService.Application.Features.Identity.Queries.GetJwks
{
    public sealed class GetJwksQueryHandler(
        ITokenProvider tokenProvider
    ) : IRequestHandler<GetJwksQuery, Result<JsonWebKeySet>>
    {
        public async Task<Result<JsonWebKeySet>> Handle(GetJwksQuery request, CancellationToken cancellationToken)
        {
            var publicJwk = tokenProvider.GetPublicKeyJwk();
            var jwks = new JsonWebKeySet();
            jwks.Keys.Add(publicJwk);
            return jwks;
        }
    }
}
