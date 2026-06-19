using Lynx.RedirectionService.Application.Common.Errors;
using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Application.Features.Urls.Dtos;
using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.RedirectionService.Application.Features.Urls.Queries.GetUrlByAlias
{
    public sealed class GetUrlByAliasQueryHandler(
        IUrlRepository urlRepository,
        ILogger<GetUrlByAliasQueryHandler> logger,
        TimeProvider timeProvider,
        IMessagePublishingService messagePublishingService
    ) : IRequestHandler<GetUrlByAliasQuery, Result<UrlDto>>
    {
        public async Task<Result<UrlDto>> Handle(GetUrlByAliasQuery request, CancellationToken cancellationToken)
        {
            var url = await urlRepository.GetUrlByAliasAsync(request.Alias, cancellationToken);

            if (url?.IsDeleted != false || url.ExpirationDate <= timeProvider.GetUtcNow())
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Url {UrlAlias} does not exist.", request.Alias);

                return ApplicationErrors.UrlDoesNotExist;
            }

            var urlDto = new UrlDto
            {
                Id = url.Id,
                Alias = url.Alias,
                LongUrl = url.LongUrl,
                ExpiresAt = url.ExpirationDate
            };
            await messagePublishingService.PublishAsync("ShortLinkVisits", urlDto, cancellationToken);
            return urlDto;
        }
    }
}
