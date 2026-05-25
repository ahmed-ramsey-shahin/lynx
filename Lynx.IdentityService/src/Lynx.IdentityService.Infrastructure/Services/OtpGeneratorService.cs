using System.Security.Cryptography;
using Lynx.IdentityService.Application.Common.Services;

namespace Lynx.IdentityService.Infrastructure.Services
{
    public class OtpGeneratorService : IOTPGeneratorService
    {
        public string GenerateResetCode(int length = 6)
        {
            int maxExclusive = (int) Math.Pow(10, length);
            int secureNumber = RandomNumberGenerator.GetInt32(0, maxExclusive);
            return secureNumber.ToString($"D{length}");
        }

        public string GenerateUrlSafeToken(int byteLength = 32)
        {
            return RandomNumberGenerator.GetHexString(byteLength);
        }
    }
}
