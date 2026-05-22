using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed class RefreshToken
    {
        public string Token { get; }
        public DateTimeOffset ExpiresOn { get; }
        public bool IsRevoked { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }
        public bool IsValid => !IsRevoked && ExpiresOn > DateTimeOffset.UtcNow;

        private RefreshToken(string token, DateTimeOffset expiresOn)
        {
            Token = token;
            ExpiresOn = expiresOn;
            IsRevoked = false;
        }

        public static Result<RefreshToken> Create(string token, DateTimeOffset expiresOn)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RefreshTokenErrors.TokenRequired;
            }

            return new RefreshToken(token, expiresOn);
        }

        public Result<Updated> Revoke(DateTimeOffset currentUtcTime)
        {
            if (IsRevoked)
            {
                return Result.Updated;
            }

            IsRevoked = true;
            RevokedAt = currentUtcTime;
            return Result.Updated;
        }
    }
}
