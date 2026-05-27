using Lynx.IdentityService.Application.Common.Interfaces;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results;
using Lynx.IdentityService.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Common.Behaviors
{
    public class IdempotencyBehavior<TRequest, TResponse>(
        ICacheService cacheService,
        ILogger<IdempotencyBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IIdempotentCommand idempotentCommand)
            {
                return await next();
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Check cache for {RequestName} and {IdempotencyKey}.", typeof(TRequest).Name, idempotentCommand.IdempotencyKey);

            var cachedResult = await cacheService.GetAsync<TResponse>(idempotentCommand.IdempotencyKey, cancellationToken);

            if (cachedResult is not null)
            {
                logger.LogInformation("Returned cached idempotent result.");
                return cachedResult;
            }

            var lockKey = $"{idempotentCommand.IdempotencyKey}_lock";
            var lockAcquired = await cacheService.TrySetAsync(lockKey, "InProgress", TimeSpan.FromMinutes(2), cancellationToken);

            if (!lockAcquired)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("{IdempotencyKey} already exists in the cache.", idempotentCommand.IdempotencyKey);

                return (dynamic) Error.Conflict("Idempotency.Conflict", "Request is already being processed.");
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("{IdempotencyKey} was not found in the cache.", idempotentCommand.IdempotencyKey);

            try
            {
                var result = await next();
                if (result.IsError)
                {
                    if (logger.IsEnabled(LogLevel.Error))
                        logger.LogError("An error happened during execution of the command. {@Errors}.", result.Errors);

                    return result;
                }

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Storing {IdempotencyKey} to cache.", idempotentCommand.IdempotencyKey);

                await cacheService.SetAsync(idempotentCommand.IdempotencyKey, result, TimeSpan.FromMinutes(3), cancellationToken);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await cacheService.RemoveAsync(lockKey, cancellationToken);
            }
        }
    }
}
