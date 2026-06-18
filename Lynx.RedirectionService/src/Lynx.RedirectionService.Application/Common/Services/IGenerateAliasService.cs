namespace Lynx.RedirectionService.Application.Common.Services
{
    public interface IGenerateAliasService
    {
        string Generate(int length=8);
    }
}
