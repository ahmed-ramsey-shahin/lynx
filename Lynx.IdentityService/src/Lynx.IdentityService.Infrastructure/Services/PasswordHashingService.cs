using Lynx.IdentityService.Application.Common.Services;

namespace Lynx.IdentityService.Infrastructure.Services
{
    public class PasswordHashingService(string pepper) : IPasswordHashingService
    {
        private const int WorkFactor = 12;

        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(nameof(password));
            }

            return BCrypt.Net.BCrypt.EnhancedHashPassword(password + pepper, WorkFactor);
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
        }
    }
}
