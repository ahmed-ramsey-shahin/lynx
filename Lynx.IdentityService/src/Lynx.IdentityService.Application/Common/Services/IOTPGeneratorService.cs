namespace Lynx.IdentityService.Application.Common.Services
{
    public interface IOTPGeneratorService
    {
        string GenerateResetCode(int length=6);
        string GenerateUrlSafeToken(int byteLength=32);
    }
}
