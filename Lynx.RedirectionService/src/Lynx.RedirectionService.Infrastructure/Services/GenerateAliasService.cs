using System.Security.Cryptography;
using Lynx.RedirectionService.Application.Common.Services;

namespace Lynx.RedirectionService.Infrastructure.Services
{
    public class GenerateAliasService : IGenerateAliasService
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public string Generate(int length = 8)
        {
            return RandomNumberGenerator.GetString(Alphabet, length);
        }
    }
}
