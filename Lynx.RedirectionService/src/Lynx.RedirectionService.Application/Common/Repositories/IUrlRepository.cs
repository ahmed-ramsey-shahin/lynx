using Lynx.RedirectionService.Domain.Urls;

namespace Lynx.RedirectionService.Application.Common.Repositories
{
    public interface IUrlRepository
    {
        Task<Url?> GetUrlByAliasAsync(string alias, CancellationToken cancellationToken=default);
        Task<bool> AliasExistsAsync(string alias, CancellationToken cancellationToken=default);
        Task<Url?> GetUrlByIdAsync(Guid id, CancellationToken cancellationToken=default);
        Task<bool> AddAsync(Url url, CancellationToken cancellationToken=default);
        Task<bool> UpdateUrlAsync(Url url, CancellationToken cancellationToken=default);
    }
}
