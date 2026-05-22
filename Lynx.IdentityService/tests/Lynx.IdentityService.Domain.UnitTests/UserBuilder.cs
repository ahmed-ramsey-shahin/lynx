using Lynx.IdentityService.Domain.Identity;

namespace Lynx.IdentityService.Domain.UnitTests
{
    public class UserBuilder
    {
        private readonly User _user;

        public UserBuilder()
        {
            _user = User.Create(
                Guid.NewGuid(),
                "email@lynx.com",
                "lynx_user",
                "VeryStrong@Password123"
            ).Value;
        }

        public UserBuilder Activated()
        {
            _user.Activate(DateTimeOffset.UtcNow);
            return this;
        }

        public UserBuilder HasRefreshTokens(int numberOfTokens=10)
        {
            for (int i = 0; i < numberOfTokens; i++)
            {
                _user.AddRefreshToken($"RandomToken{numberOfTokens}", DateTimeOffset.UtcNow);
            }

            return this;
        }

        public User Build()
        {
            _user.ClearEvents();
            return _user;
        }
    }
}
