using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Domain.Common.Results;
using Lynx.IdentityService.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser
{
    public class CreateUserCommandHandler(
        ILogger<CreateUserCommandHandler> logger,
        IUserRepository userRepo
    ) : IRequestHandler<CreateUserCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var isEmailUnique = await userRepo.IsEmailUniqueAsync(request.Email, cancellationToken);

            if (!isEmailUnique)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Request could not be processed {Email} is not unique.", request.Email);
                return ApplicationErrors.EmailAlreadyExists;
            }

            var isUsernameUnique = await userRepo.IsUsernameUniqueAsync(request.Username, cancellationToken);

            if (!isUsernameUnique)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("Request could not be processed {Username} is not unique.", request.Username);
                return ApplicationErrors.UsernameAlreadyExists;
            }

            var creationResult = User.Create(Guid.NewGuid(), request.Email, request.Username, request.Password);

            if (creationResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Request could not be processed. {@Errors}.", creationResult.Errors);

                return creationResult.Errors!;
            }

            await userRepo.AddAsync(creationResult.Value, cancellationToken);
            return creationResult.Value.Id;
        }
    }
}
