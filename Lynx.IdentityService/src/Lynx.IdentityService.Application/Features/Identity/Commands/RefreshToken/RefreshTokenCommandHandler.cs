using System.Security.Claims;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler(
        ILogger<RefreshTokenCommandHandler> logger,
        IUserRepository userRepo,
        ITokenProvider tokenProvider,
        TimeProvider timeProvider
    ) : IRequestHandler<RefreshTokenCommand, Result<TokenDto>>
    {
        public async Task<Result<TokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var now = timeProvider.GetUtcNow();
            var principal = tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

            if (principal is null)
            {
                logger.LogWarning("Expired access token is not valid.");
                return ApplicationErrors.ExpiredAccessTokenInvalid;
            }

            var username = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (username is null)
            {
                logger.LogWarning("Invalid user claim.");
                return ApplicationErrors.UsernameClaimInvalid;
            }

            var user = await userRepo.GetUserByUsernameAsync(username, cancellationToken);

            if (user is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("No user was found with {Username}.", username);
                return ApplicationErrors.UserNotFound;
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(token => token.Token == request.RefreshToken);

            if (refreshToken is null || refreshToken.ExpiresOn < now || refreshToken.IsRevoked)
            {
                logger.LogWarning("Refresh token has expired.");
                return ApplicationErrors.RefreshTokenExpired;
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
                    logger.LogError("Could not generate token for {Username}. {@Errors}.", username, generateTokenResult.Errors);

                return generateTokenResult.Errors!;
            }

            user.RemoveRefreshToken(refreshToken.Token);
            user.RemoveExpiredRefreshTokens(now);
            user.AddRefreshToken(generateTokenResult.Value.RefreshToken, generateTokenResult.Value.ExpiresAt);
            await userRepo.UpdateAsync(user, cancellationToken);
            return generateTokenResult;
        }
    }
}
