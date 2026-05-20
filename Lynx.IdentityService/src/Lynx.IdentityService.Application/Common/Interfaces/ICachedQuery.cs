using MediatR;

namespace Lynx.IdentityService.Application.Common.Interfaces
{
    public interface ICachedQuery
    {
        string CacheKey { get; }
        TimeSpan Expiration { get; }
    }

    public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;
}
