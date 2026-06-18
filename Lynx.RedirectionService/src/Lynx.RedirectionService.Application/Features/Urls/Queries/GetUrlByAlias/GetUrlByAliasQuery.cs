using Lynx.RedirectionService.Application.Common.Interfaces;
using Lynx.RedirectionService.Application.Features.Urls.Dtos;
using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;

namespace Lynx.RedirectionService.Application.Features.Urls.Queries.GetUrlByAlias
{
    public sealed record GetUrlByAliasQuery : ICachedQuery, IRequest<Result<UrlDto>>
    {
        public string Alias { get; init; } = null!;
        public string CacheKey => Alias;
        public TimeSpan Expiration => TimeSpan.FromMinutes(15);
    }
}
