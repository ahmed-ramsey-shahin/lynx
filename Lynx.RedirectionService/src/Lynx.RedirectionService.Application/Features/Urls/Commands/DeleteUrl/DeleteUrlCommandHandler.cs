using Lynx.RedirectionService.Application.Common.Errors;
using Lynx.RedirectionService.Application.Common.Repositories;
using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.RedirectionService.Application.Features.Urls.Commands.DeleteUrl
{
    public sealed class DeleteUrlCommandHandler(
        IUrlRepository urlRepository,
        ILogger<DeleteUrlCommandHandler> logger
    ) : IRequestHandler<DeleteUrlCommand, Result<Deleted>>
    {
        public async Task<Result<Deleted>> Handle(DeleteUrlCommand request, CancellationToken cancellationToken)
        {
            var url = await urlRepository.GetUrlByIdAsync(request.UrlId, cancellationToken);

            if (url is null)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Url {UrlId} does not exist.", request.UrlId);

                return ApplicationErrors.UrlDoesNotExist;
            }

            if (url.UserId != request.UserId)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("User {UserId} attempted to delete Url {UrlId} without ownership.", request.UserId, request.UrlId);
                return ApplicationErrors.UrlNotOwned;
            }

            var deletionResult = url.Delete();

            if (deletionResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Url can not be deleted. {@Errors}.", deletionResult.Errors);

                return deletionResult.Errors!;
            }

            await urlRepository.UpdateUrlAsync(url, cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Url {UrlId} deleted successfully.", url.Id);

            return Result.Deleted;
        }
    }
}
