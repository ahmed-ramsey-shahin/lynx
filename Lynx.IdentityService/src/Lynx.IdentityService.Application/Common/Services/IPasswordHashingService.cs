namespace Lynx.IdentityService.Application.Common.Services
{
    public interface IPasswordHashingService
    {
        string Hash(string password);
        string Verify(string password, string hash);
    }
}
