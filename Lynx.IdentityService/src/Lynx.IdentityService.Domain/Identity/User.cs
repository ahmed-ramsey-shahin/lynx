using Lynx.IdentityService.Domain.Common;
using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed class User : EventfulEntity
    {
        public Guid Id { get; }
        public string Email { get; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool IsActivated { get; private set; }
        public DateTimeOffset? ActivationDate { get; private set; }
        private readonly List<RefreshToken> _refreshTokens = [];
        public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

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
            user.AddEvent(new UserRegistered(id));
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
            AddEvent(new PasswordChanged(Id));
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

            Username = username;
            AddEvent(new UsernameChanged(Id));
            return Result.Updated;
        }

        public Result<Updated> Activate(DateTimeOffset activationDate)
        {
            if (IsActivated)
            {
                return UserErrors.AlreadyActivated;
            }

            IsActivated = true;
            ActivationDate = activationDate;
            AddEvent(new UserActivated(Id));
            return Result.Updated;
        }

        public Result<Deleted> Delete()
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            IsDeleted = true;
            AddEvent(new UserDeleted(Id));
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

        public Result<Updated> Revoke(string token, DateTimeOffset currentUtcTime)
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            var refreshToken = _refreshTokens.FirstOrDefault(refreshToken => refreshToken.Token.Equals(token));

            if (refreshToken is null)
            {
                return UserErrors.TokenNotFound;
            }

            var revokeResult = refreshToken.Revoke(currentUtcTime);

            if (revokeResult.IsError)
            {
                return revokeResult.Errors!;
            }

            return revokeResult;
        }

        public Result<Updated> RevokeAllTokens(DateTimeOffset currentUtcTime)
        {
            if (!IsActivated)
            {
                return UserErrors.NotActivated;
            }

            foreach (var token in RefreshTokens)
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
