using FluentAssertions;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Common.Settings;
using Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class CreateUserCommandTests
    {
        private readonly Mock<ILogger<CreateUserCommandHandler>> _logger = new();
        private readonly Mock<IUserRepository> _userRepo = new(MockBehavior.Strict);
        private readonly Mock<IOTPGeneratorService> _generatorService = new(MockBehavior.Strict);
        private readonly Mock<IEmailService> _emailService = new(MockBehavior.Strict);
        private readonly Mock<ICacheService> _cacheService = new(MockBehavior.Strict);
        private readonly Mock<IPasswordHashingService> _hashingService = new(MockBehavior.Strict);
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandTests()
        {
            _logger.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
            var fakeUrls = new ClientUrlOptions
            {
                ActivateAccountUrl = "http://fake.com",
                ResetPasswordUrl = "http://fake.com"
            };
            var optionsMock = Options.Create(fakeUrls);
            _handler = new(
                _logger.Object,
                _userRepo.Object,
                _generatorService.Object,
                _emailService.Object,
                _cacheService.Object,
                optionsMock,
                _hashingService.Object
            );
        }

        [Fact]
        public async Task Handle_Should_AddUserToRepository_WhenParametersAreValidAndUsernameAndEmailAreUnique()
        {
            // Arrange
            const string email = "email@lynx.com";
            const string username = "lynx_user";
            const string password = "VeryStrong@Password123";
            const string hashedPassword = "VeryStrongHash";
            const string activationCode = "f22349513bab9d8f";
            string idempotencyKey = Guid.NewGuid().ToString();
            var command = new CreateUserCommand()
            {
                Email = email,
                Username = username,
                Password = password,
                IdempotencyKey = idempotencyKey
            };
            // _userRepo
            _userRepo.Setup(repo => repo.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>( )))
                .ReturnsAsync(true);
            _userRepo.Setup(repo => repo.IsUsernameUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>( )))
                .ReturnsAsync(true);
            _userRepo.Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // _generatorService
            _generatorService.Setup(service => service.GenerateUrlSafeToken(It.IsAny<int>()))
                .Returns(activationCode);

            // _emailService
            _emailService.Setup(
                service => service.SendEmailAsync(
                    email,
                    username,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            ).Returns(Task.CompletedTask);

            // _cacheService
            _cacheService.Setup(
                service => service.SetAsync(
                    $"activation-codes:{activationCode}",
                    It.IsAny<Guid>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()
                )
            ).Returns(Task.CompletedTask);

            // _hashingService
            _hashingService.Setup(
                service => service.Hash(
                    It.IsAny<string>()
                )
            ).Returns(hashedPassword);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _emailService.Verify(
                service => service.SendEmailAsync(
                    email,
                    username,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once()
            );
            _cacheService.Verify(
                service => service.SetAsync(
                    $"activation-codes:{activationCode}",
                    It.IsAny<Guid>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once()
            );
            _userRepo.Verify(
                repo => repo.IsEmailUniqueAsync(
                    email,
                    It.IsAny<CancellationToken>()
                ),
                Times.Once()
            );
            _userRepo.Verify(
                repo => repo.IsUsernameUniqueAsync(
                    username,
                    It.IsAny<CancellationToken>()
                ),
                Times.Once()
            );
            _userRepo.Verify(repo => repo.AddAsync(It.Is<User>(user =>
                user.Email == email &&
                user.Username == username &&
                user.Password == hashedPassword
            ), It.IsAny<CancellationToken>()), Times.Once());
            _hashingService.Verify(service => service.Hash(password), Times.Once());
        }
    }
}
