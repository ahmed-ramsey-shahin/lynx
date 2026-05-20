using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Queries.GenerateToken
{
    public sealed class GenerateTokenQueryHandler(
        ILogger<GenerateTokenQueryHandler> logger,
        ITokenProvider tokenProvider,
        IIdentityService identityService
    ) : IRequestHandler<GenerateTokenQuery, Result<TokenDto>>
    {
        public async Task<Result<TokenDto>> Handle(GenerateTokenQuery request, CancellationToken cancellationToken)
        {
            var userResult = await identityService.AuthenticateAsync(request.Username, request.Password, cancellationToken);

            if (userResult.IsError)
            {
                if(logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Could not authenticate {Username}. {@Errors}.", request.Username, userResult.Errors);

                return userResult.Errors!;
            }

            var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(userResult.Value, cancellationToken);

            if (generateTokenResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Could not generate token for {Username}. {@Errors}.", request.Username, generateTokenResult.Errors);

                return generateTokenResult.Errors!;
            }

            return generateTokenResult;
        }
    }
}
