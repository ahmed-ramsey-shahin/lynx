using System.Security.Claims;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class TokenProviderMockBuilder
    {
        public Mock<ITokenProvider> Mock { get; } = new(MockBehavior.Strict);
        public ITokenProvider Object => Mock.Object;

        public TokenProviderMockBuilder WithJwtToken(UserDto? user=null, TokenDto? resultToken=null)
        {
            Mock.Setup(provider => provider.GenerateJwtToken(It.Is<UserDto>(v => user == null || user == v)))
                .Returns(resultToken);
            return this;
        }

        public TokenProviderMockBuilder WithPrincipal(string? token=null, ClaimsPrincipal? principal=null)
        {
            Mock.Setup(provider => provider.GetPrincipalFromExpiredToken(It.Is<string>(v => token == null || token == v)))
                .Returns(principal);
            return this;
        }

        public TokenProviderMockBuilder WithPublicKeyJwk(JsonWebKey webKey)
        {
            Mock.Setup(provider => provider.GetPublicKeyJwk()).Returns(webKey);
            return this;
        }
    }
}
