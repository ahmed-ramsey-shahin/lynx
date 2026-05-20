namespace Lynx.IdentityService.Application.Common.Services
{
    public interface IPasswordGenerationService
    {
        string Generate(int length=8);
    }
}
