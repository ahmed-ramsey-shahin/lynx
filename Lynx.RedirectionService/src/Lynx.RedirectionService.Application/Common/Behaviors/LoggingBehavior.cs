using Lynx.RedirectionService.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.RedirectionService.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Request: {Name} {@Request}", requestName, request);

            return await next();
        }
    }
}
