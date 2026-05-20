using System.Diagnostics;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Common.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse>(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IUserService userService
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        private readonly Stopwatch _timer = new();

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _timer.Start();
            var response = await next(cancellationToken);
            _timer.Stop();
            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;
                var userId = userService.UserId ?? string.Empty;

                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Long event request: {Name} ({ElapsedMilliseconds} milliseconds) {UserId} {@Request}.", requestName, elapsedMilliseconds, userId, request);
            }

            return response;
        }
    }
}
