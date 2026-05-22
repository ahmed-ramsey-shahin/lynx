using FluentAssertions;
using Lynx.IdentityService.Application.Features.Identity.Queries.GetJwks;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class GetJwksQueryTests
    {
        [Fact]
        public async Task Handler_Should_ReturnJwksWithPublicKey()
        {
            // Arrange
            var expectedJwk = new JsonWebKey
            {
                KeyId = "lynx-auth-key-1"
            };
            var tokenProvider = new TokenProviderMockBuilder().WithPublicKeyJwk(expectedJwk);
            var handler = new GetJwksQueryHandler(tokenProvider.Object);
            var query = new GetJwksQuery();

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Keys.Should().ContainSingle()
                .Which.KeyId.Should().Be(expectedJwk.KeyId);
            tokenProvider.Mock.Verify(provider => provider.GetPublicKeyJwk(), Times.Once());
        }
    }
}
