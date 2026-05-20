using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IUserService userService
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = userService.UserId;

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Request: {Name} {UserId} {@Request}", requestName, userId, request);

            return await next(cancellationToken);
        }
    }
}
