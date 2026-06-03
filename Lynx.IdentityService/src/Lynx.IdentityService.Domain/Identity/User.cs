using Lynx.IdentityService.Domain.Common;
using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed class User : EventfulEntity
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = null!;
        public string Username { get; private set; } = null!;
        public string Password { get; private set; } = null!;
        public bool IsActivated { get; private set; }
        public DateTimeOffset? ActivationDate { get; private set; }
        private List<RefreshToken> _refreshTokens = [];
        public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        private User()
        {}

        private User(Guid id, string email, string username, string password)
        {
            Id = id;
            Email = email;
            Password = password;
            Username = username;
            IsActivated = false;
            ActivationDate = null;
        }

        public static Result<User> Create(Guid id, string email, string username, string password)
        {
            if (id == Guid.Empty)
            {
                return UserErrors.IdRequired;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return UserErrors.EmailRequired;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                return UserErrors.UsernameRequired;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return UserErrors.PasswordRequired;
            }

            var user = new User(id, email, username, password);
            user.AddEvent(new UserRegisteredEvent(id, email, username));
            return user;
        }

        public Result<Updated> ChangePassword(string password)
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return UserErrors.PasswordRequired;
            }

            Password = password;
            AddEvent(new PasswordChangedEvent(Id, Email, Username));
            return Result.Updated;
        }

        public Result<Updated> ChangeUsername(string username)
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                return UserErrors.UsernameRequired;
            }

            AddEvent(new UsernameChangedEvent(Id, Email, Username, username));
            Username = username;
            return Result.Updated;
        }

        public Result<Updated> Activate(DateTimeOffset activationDate)
        {
            if (IsActivated)
            {
                return Result.Updated;
            }

            IsActivated = true;
            ActivationDate = activationDate;
            AddEvent(new UserActivatedEvent(Id, Email, Username));
            return Result.Updated;
        }

        public Result<Deleted> Delete(DateTimeOffset deletedAt)
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            IsDeleted = true;
            DeletedAt = deletedAt;
            AddEvent(new UserDeletedEvent(Id, Email, Username));
            return Result.Deleted;
        }

        public Result<RefreshToken> AddRefreshToken(
            string token,
            DateTimeOffset expiresOn
        )
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            var creationResult = RefreshToken.Create(token, expiresOn);

            if (creationResult.IsError)
            {
                return creationResult.Errors!;
            }

            _refreshTokens.Add(creationResult.Value);
            return creationResult;
        }

        public Result<Updated> RevokeAllTokens(DateTimeOffset currentUtcTime)
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            foreach (var token in RefreshTokens.Where(token => !token.IsRevoked))
            {
                token.Revoke(currentUtcTime);
            }

            return Result.Updated;
        }

        public Result<Updated> RemoveExpiredRefreshTokens(DateTimeOffset currentUtcTime)
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            _refreshTokens.RemoveAll(token => token.ExpiresOn < currentUtcTime);
            return Result.Updated;
        }

        public Result<Updated> RemoveRefreshToken(string token)
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            _refreshTokens.RemoveAll(rtoken => rtoken.Token.Equals(token));
            return Result.Updated;
        }
    }
}
