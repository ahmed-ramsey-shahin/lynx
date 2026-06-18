using Lynx.RedirectionService.Domain.Urls;

namespace Lynx.RedirectionService.Application.Common.Repositories
{
    public interface IUrlRepository
    {
        Task<Url?> GetUrlByAlias(string alias, CancellationToken cancellationToken=default);
        Task<bool> AliasExistsAsync(string alias, CancellationToken cancellationToken=default);
        Task<Url?> GetUrlById(Guid id, CancellationToken cancellationToken=default);
        Task<bool> AddAsync(Url url, CancellationToken cancellationToken=default);
        Task<bool> UpdateUrl(Url url, CancellationToken cancellationToken=default);
    }
}
