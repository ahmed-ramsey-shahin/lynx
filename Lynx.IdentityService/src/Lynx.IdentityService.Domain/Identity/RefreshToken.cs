using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed class RefreshToken
    {
        public string Id { get; }
        public string Token { get; }
        public string UserId { get; }
        public DateTimeOffset ExpiresOn { get; }
        public bool IsRevoked { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }
        public bool IsValid => !IsRevoked && ExpiresOn > DateTimeOffset.UtcNow;

        private RefreshToken(string id, string token, string userId, DateTimeOffset expiresOn)
        {
            Id = id;
            Token = token;
            UserId = userId;
            ExpiresOn = expiresOn;
            IsRevoked = false;
        }

        public static Result<RefreshToken> Create(string id, string token, string userId, DateTimeOffset expiresOn)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RefreshTokenErrors.IdRequired;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return RefreshTokenErrors.TokenRequired;
            }

            if (string.IsNullOrEmpty(userId))
            {
                return RefreshTokenErrors.UserIdRequired;
            }

            if (expiresOn <= DateTimeOffset.UtcNow)
            {
                return RefreshTokenErrors.ExpirationInvalid;
            }

            return new RefreshToken(id, token, userId, expiresOn);
        }

        public Result<Updated> Revoke(TimeProvider timeProvider)
        {
            if (IsRevoked)
            {
                return Result.Updated;
            }

            IsRevoked = true;
            RevokedAt = timeProvider.GetUtcNow();
            return Result.Updated;
        }
    }
    }
