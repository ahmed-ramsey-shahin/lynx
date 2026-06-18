using MediatR;

namespace Lynx.RedirectionService.Application.Common.Interfaces
{
    public interface ICachedQuery
    {
        string CacheKey { get; }
        TimeSpan Expiration { get; }
    }

    public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;
}
