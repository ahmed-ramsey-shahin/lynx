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
            var alias = request.CustomAlias ?? aliasService.Generate();
            var expirationInDays = request.ExpirationInDays ?? 30;

            if (await urlRepository.AliasExistsAsync(alias, cancellationToken))
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("{Alias} already exists.", alias);

                return ApplicationErrors.AliasAlreadyExists;
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
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Url could not be created. {@Errors}.", urlCreationResult.Errors);

                return urlCreationResult.Errors!;
            }

            await urlRepository.AddAsync(urlCreationResult.Value, cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Url {Id} was created successfully.", urlCreationResult.Value.Id);

            return urlCreationResult.Value.Id;
        }
    }
}
