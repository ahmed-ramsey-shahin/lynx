using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Domain.Identity
{
    public static class RefreshTokenErrors
    {
        public static Error IdRequired => Error.Validation("RefreshToken.IdRequired", "Refresh token ID is required.");
        public static Error TokenRequired => Error.Validation("RefreshToken.TokenRequired", "Refresh token is required.");
        public static Error UserIdRequired => Error.Validation("RefreshToken.UserIdRequired", "The user ID associated with this refresh token is required.");
        public static Error ExpirationInvalid => Error.Validation("RefreshToken.ExpirationInvalid", "The expiration date of the refresh token must be in the future.");
    }
}
