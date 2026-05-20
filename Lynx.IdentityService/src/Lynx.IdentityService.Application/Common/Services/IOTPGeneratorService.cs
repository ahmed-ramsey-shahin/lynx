namespace Lynx.IdentityService.Application.Common.Services
{
    public interface IOTPGeneratorService
    {
        string GenerateResetCode();
    }
}
