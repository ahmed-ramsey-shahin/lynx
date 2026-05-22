using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.GenerateToken
{
    public sealed class GenerateTokenCommandHandler(
        ILogger<GenerateTokenCommandHandler> logger,
        ITokenProvider tokenProvider,
        IUserRepository userRepo,
        IPasswordHashingService hashingService,
        TimeProvider timeProvider
    ) : IRequestHandler<GenerateTokenCommand, Result<TokenDto>>
    {
        public async Task<Result<TokenDto>> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
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

            var generatedToken = tokenProvider.GenerateJwtToken(new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                UserId = user.Id
            }, cancellationToken);

            if (generatedToken is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Could not generate token for {Username}.", request.Username);

                return ApplicationErrors.TokenGenerationFailed;
            }

            user.RemoveExpiredRefreshTokens(timeProvider.GetUtcNow());
            user.AddRefreshToken(generatedToken.RefreshToken, generatedToken.ExpiresAt);
            await userRepo.UpdateAsync(user, cancellationToken);
            return generatedToken;
        }
    }
}
