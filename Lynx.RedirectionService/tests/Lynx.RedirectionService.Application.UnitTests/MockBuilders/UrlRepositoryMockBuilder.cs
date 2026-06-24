using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Domain.Urls;
using Moq;

namespace Lynx.RedirectionService.Application.UnitTests.MockBuilders
{
    public class UrlRepositoryMockBuilder
    {
        public Mock<IUrlRepository> Mock { get; } = new(MockBehavior.Strict);
        public IUrlRepository Object => Mock.Object;

        public UrlRepositoryMockBuilder UrlByAlias(string? alias=null, Url? url=null)
        {
            if (alias is null)
            {
                Mock.Setup(repo => repo.GetUrlByAliasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(url);
            }
            else
            {
                Mock.Setup(repo => repo.GetUrlByAliasAsync(alias, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(url);
            }

            return this;
        }

        public UrlRepositoryMockBuilder UrlById(Guid? id=null, Url? url=null)
        {
            if (id is null)
            {
                Mock.Setup(repo => repo.GetUrlByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(url);
            }
            else
            {
                Mock.Setup(repo => repo.GetUrlByIdAsync(id.Value, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(url);
            }

            return this;
        }

        public UrlRepositoryMockBuilder WithAliasExists(string alias)
        {
            Mock.Setup(repo => repo.AliasExistsAsync(alias, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return this;
        }

        public UrlRepositoryMockBuilder WithAliasUnique(string alias)
        {
            Mock.Setup(repo => repo.AliasExistsAsync(alias, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            return this;
        }

        public UrlRepositoryMockBuilder WithSuccessfullDatabaseAdd()
        {
            Mock.Setup(repo => repo.AddAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return this;
        }

        public UrlRepositoryMockBuilder WithSuccessfullDatabaseUpdate()
        {
            Mock.Setup(repo => repo.UpdateUrlAsync(It.IsAny<Url>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return this;
        }
    }
}
