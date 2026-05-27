using Lynx.IdentityService.Application.Common.Interfaces;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Common.Behaviors
{
    public class CachingBehavior<TRequest, TResponse>(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICachedQuery cachedQuery)
            {
                return await next();
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Checking cache for {RequestName} and {CacheKey}.", typeof(TRequest).Name, cachedQuery.CacheKey);

            var cacheResult = await cacheService.GetAsync<TResponse>(cachedQuery.CacheKey, cancellationToken);

            if (cacheResult is not null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("{CacheKey} was found in the cache.", cachedQuery.CacheKey);

                return cacheResult;
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("{CacheKey} was not found in cache.", cachedQuery.CacheKey);

            var result = await next();

            if (result.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("An error happened during execution of the command. {@Errors}.", result.Errors);

                return result;
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Storing {CacheKey} to cache.", cachedQuery.CacheKey);

            await cacheService.SetAsync(cachedQuery.CacheKey, result, cachedQuery.Expiration, cancellationToken);
            return result;
        }
    }
}
