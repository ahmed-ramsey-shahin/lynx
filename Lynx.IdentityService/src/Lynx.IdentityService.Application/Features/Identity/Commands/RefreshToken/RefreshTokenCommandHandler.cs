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

            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdString is null)
            {
                logger.LogWarning("Invalid user claim.");
                return ApplicationErrors.UsernameClaimInvalid;
            }

            var userId = Guid.Parse(userIdString);

            var user = await userRepo.GetUserByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("No user was found with {Username}.", userId);
                return ApplicationErrors.UserNotFound;
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(token => token.Token == request.RefreshToken);

            if (refreshToken is null || refreshToken.ExpiresOn < now || refreshToken.IsRevoked)
            {
                logger.LogWarning("Refresh token has expired.");
                return ApplicationErrors.RefreshTokenExpired;
            }

            var generatedToken = tokenProvider.GenerateJwtToken(new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                UserId = user.Id
            });

            if (generatedToken is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Could not generate token for {Username}.", userId);

                return ApplicationErrors.TokenGenerationFailed;
            }

            user.RemoveRefreshToken(refreshToken.Token);
            user.RemoveExpiredRefreshTokens(now);
            user.AddRefreshToken(generatedToken.RefreshToken, generatedToken.ExpiresAt);
            await userRepo.UpdateAsync(user, cancellationToken);
            return generatedToken;
        }
    }
}
