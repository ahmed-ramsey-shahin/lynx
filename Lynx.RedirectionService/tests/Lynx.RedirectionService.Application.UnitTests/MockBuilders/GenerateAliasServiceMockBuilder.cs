using Lynx.RedirectionService.Application.Common.Services;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests.MockBuilders
{
    public class GenerateAliasServiceMockBuilder
    {
        public Mock<IGenerateAliasService> Mock { get; } = new(MockBehavior.Strict);
        public IGenerateAliasService Object => Mock.Object;

        public GenerateAliasServiceMockBuilder WithAlias(string alias)
        {
            Mock.Setup(service => service.Generate(It.IsAny<int>())).Returns(alias);
            return this;
        }
    }
}
