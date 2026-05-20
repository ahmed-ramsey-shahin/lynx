using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
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
        IUserRepository userRepo,
        IPasswordHashingService hashingService,
        TimeProvider timeProvider
    ) : IRequestHandler<GenerateTokenQuery, Result<TokenDto>>
    {
        public async Task<Result<TokenDto>> Handle(GenerateTokenQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepo.GetUserByUsernameAsync(request.Username, cancellationToken);

            if (user is null)
            {
                if(logger.IsEnabled(LogLevel.Error))
                    logger.LogError("User {Username} not found.", request.Username);

                return ApplicationErrors.CredentialsInvalid;
            }

            if (!hashingService.Verify(request.Password, user.Password))
            {
                logger.LogWarning("User {Username} could not be authenticated. Credentials invalid.", request.Username);
                return ApplicationErrors.CredentialsInvalid;
            }

            var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                UserId = user.Id
            }, cancellationToken);

            if (generateTokenResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Could not generate token for {Username}. {@Errors}.", request.Username, generateTokenResult.Errors);

                return generateTokenResult.Errors!;
            }

            user.RemoveExpiredRefreshTokens(timeProvider.GetUtcNow());
            user.AddRefreshToken(generateTokenResult.Value.RefreshToken, generateTokenResult.Value.ExpiresAt);
            await userRepo.UpdateAsync(user, cancellationToken);
            return generateTokenResult;
        }
    }
}
