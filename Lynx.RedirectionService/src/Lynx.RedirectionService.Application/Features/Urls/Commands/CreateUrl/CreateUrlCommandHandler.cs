using Lynx.RedirectionService.Application.Common.Errors;
using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Application.Common.Services;
using Lynx.RedirectionService.Domain.Common.Results;
using Lynx.RedirectionService.Domain.Urls;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.RedirectionService.Application.Features.Urls.Commands.CreateUrl
{
    public sealed class CreateUrlCommandHandler(
        ILogger<CreateUrlCommandHandler> logger,
        IUrlRepository urlRepository,
        IGenerateAliasService aliasService,
        TimeProvider timeProvider
    ) : IRequestHandler<CreateUrlCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateUrlCommand request, CancellationToken cancellationToken)
        {
            var expirationInDays = request.ExpirationInDays ?? 30;
            var isCustomAlias = !string.IsNullOrWhiteSpace(request.CustomAlias);
            var alias = request.CustomAlias ?? aliasService.Generate();
            var attempts = 0;
            var maxAttempts = isCustomAlias ? 1 : 3;

            while (attempts < maxAttempts)
            {
                if (await urlRepository.AliasExistsAsync(alias, cancellationToken))
                {
                    if (isCustomAlias)
                    {
                        if (logger.IsEnabled(LogLevel.Warning))
                            logger.LogWarning("User attempted to claim existing alias {Alias}.", alias);
                        return ApplicationErrors.AliasAlreadyExists;
                    }

                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Collision detected for generated alias {Alias}. Regnerating...", alias);
                    alias = aliasService.Generate();
                    attempts++;
                    continue;
                }

                break;
            }

            if (attempts >= maxAttempts)
            {
                logger.LogError("Failed to generate a unique alias after {Max} attempts.", maxAttempts);
                return ApplicationErrors.AliasGenerationFailed;
            }

            var urlCreationResult = Url.Create(
                Guid.NewGuid(),
                request.UserId,
                request.LongUrl,
                alias,
                timeProvider.GetUtcNow().AddDays(expirationInDays),
                timeProvider
            );

            if (urlCreationResult.IsError)
            {
                logger.LogError("Url could not be created. {@Errors}.", urlCreationResult.Errors);
                return urlCreationResult.Errors!;
            }

            var url = urlCreationResult.Value;
            await urlRepository.AddAsync(url, cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Url {Id} with alias {Alias} was created successfully.", url.Id, url.Alias);

            return url.Id;
        }
    }
}
