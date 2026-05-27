using FluentValidation;
using Lynx.IdentityService.Domain.Common.Results;
using Lynx.IdentityService.Domain.Common.Results.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(
        ILogger<ValidationBehavior<TRequest, TResponse>> logger,
        IValidator<TRequest>? validator=null
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (validator is null)
            {
                return await next();
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Validating {RequestName}.", typeof(TRequest).Name);

            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.IsValid)
            {
                logger.LogInformation("Validation successfull");
                return await next();
            }

            var errors = validationResult.Errors
                .ConvertAll(
                    error => Error.Validation(
                        code: error.PropertyName,
                        description: error.ErrorMessage
                    )
                );
            return (dynamic) errors;
        }
    }
}
