namespace Lynx.IdentityService.Application.Common.Services
{
    public interface IPasswordHashingService
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}
