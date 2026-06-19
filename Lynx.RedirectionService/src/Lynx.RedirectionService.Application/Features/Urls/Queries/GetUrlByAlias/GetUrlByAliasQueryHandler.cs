using Lynx.RedirectionService.Application.Common.Errors;
using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Application.Features.Urls.Dtos;
using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.RedirectionService.Application.Features.Urls.Queries.GetUrlByAlias
{
    public sealed class GetUrlByAliasQueryHandler(
        IUrlRepository urlRepository,
        ILogger<GetUrlByAliasQueryHandler> logger
    ) : IRequestHandler<GetUrlByAliasQuery, Result<UrlDto>>
    {
        public async Task<Result<UrlDto>> Handle(GetUrlByAliasQuery request, CancellationToken cancellationToken)
        {
            var url = await urlRepository.GetUrlByAliasAsync(request.Alias, cancellationToken);

            if (url is null)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Url {UrlAlias} does not exist.", request.Alias);

                return ApplicationErrors.UrlDoesNotExist;
            }

            return new UrlDto
            {
                Alias = url.Alias,
                LongUrl = url.LongUrl,
                ExpiresAt = url.ExpirationDate
            };
        }
    }
}
