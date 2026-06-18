using Lynx.RedirectionService.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.RedirectionService.Application.Common.Behaviors
{
    public class UnhandledExceptionBehavior<TRequest, TResponse>(
        ILogger<TRequest> logger
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch(Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Request: Unhandled exception for request {Name} {@Request}", requestName, request);

                throw;
            }
        }
    }
}
