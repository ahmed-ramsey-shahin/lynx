using Lynx.IdentityService.Domain.Common;
using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed class User : EventfulEntity
    {
        public string Id { get; }
        public string Email { get; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool IsActivated { get; private set; }
        public DateTimeOffset? ActivationDate { get; private set; }
        private readonly List<RefreshToken> _refreshTokens = [];
        public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        private User(string id, string email, string username, string password)
        {
            Id = id;
            Email = email;
            Password = password;
            Username = username;
            IsActivated = false;
            ActivationDate = null;
        }

        public static Result<User> Create(string id, string email, string username, string password)
        {
            if (string.IsNullOrEmpty(id))
            {
                return UserErrors.IdRequired;
            }

            if (string.IsNullOrEmpty(email))
            {
                return UserErrors.EmailRequired;
            }

            if (string.IsNullOrEmpty(username))
            {
                return UserErrors.UsernameRequired;
            }

            if (string.IsNullOrEmpty(password))
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

            if (string.IsNullOrEmpty(password))
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

            if (string.IsNullOrEmpty(username))
            {
                return UserErrors.UsernameRequired;
            }

            Username = username;
            AddEvent(new UsernameChanged(Id));
            return Result.Updated;
        }

        public Result<Updated> Activate(TimeProvider timeProvider)
        {
            if (IsActivated)
            {
                return UserErrors.AlreadyActivated;
            }

            IsActivated = true;
            ActivationDate = timeProvider.GetUtcNow();
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
    }
}
