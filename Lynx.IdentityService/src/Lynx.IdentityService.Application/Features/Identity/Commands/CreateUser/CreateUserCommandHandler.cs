using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Common.Settings;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Common.Results;
using Lynx.IdentityService.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser
{
    public sealed class CreateUserCommandHandler(
        ILogger<CreateUserCommandHandler> logger,
        IUserRepository userRepo,
        IOTPGeneratorService generatorService,
        IEmailBackgroundQueue emailQueue,
        ICacheService cacheService,
        IOptions<ClientUrlOptions> options,
        IPasswordHashingService hashingService
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

            var userId = Guid.NewGuid();
            var hashedPassword = hashingService.Hash(request.Password);
            var creationResult = User.Create(userId, request.Email, request.Username, hashedPassword);

            if (creationResult.IsError)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Request could not be processed. {@Errors}.", creationResult.Errors);

                return creationResult.Errors!;
            }

            await userRepo.AddAsync(creationResult.Value, cancellationToken);
            var activationToken = generatorService.GenerateUrlSafeToken();
            await emailQueue.QueueEmailAsync(
                new EmailJob(
                    request.Email,
                    request.Username,
                    "Lynx Account Activation",
                    $@"Please visit ${options.Value.ActivateAccountUrl} to activate your account.
                    The activation code is: {activationToken}."
                ),
                cancellationToken
            );
            await cacheService.SetAsync($"activation-codes:{activationToken}", userId, TimeSpan.FromHours(2), cancellationToken);
            return creationResult.Value.Id;
        }
    }
}
