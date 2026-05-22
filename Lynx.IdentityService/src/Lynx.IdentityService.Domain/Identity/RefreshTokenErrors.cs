using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Domain.Identity
{
    public static class RefreshTokenErrors
    {
        public static Error TokenRequired => Error.Validation("RefreshToken.TokenRequired", "Refresh token is required.");
    }
}
