using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Queries.GetUser
{
    public sealed class GetUserQueryHandler(
        ILogger<GetUserQueryHandler> logger,
        IUserRepository userRepo,
        IUserService userService
    ) : IRequestHandler<GetUserQuery, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepo.GetUserByIdAsync(userService.UserId!.Value, cancellationToken);

            if (user is null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("No user was found with {UserId}.", userService.UserId);

                return ApplicationErrors.UserNotFound;
            }

            return new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                UserId = user.Id
            };
        }
    }
}
