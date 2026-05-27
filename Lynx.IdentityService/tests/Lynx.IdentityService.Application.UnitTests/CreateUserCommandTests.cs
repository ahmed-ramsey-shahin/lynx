using FluentAssertions;
using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Common.Settings;
using Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class CreateUserCommandTests
    {
        private readonly Mock<ILogger<CreateUserCommandHandler>> _logger = new(MockBehavior.Loose);
        private readonly IOptions<ClientUrlOptions> _options;
        private CreateUserCommandHandler? _handler;

        public CreateUserCommandTests()
        {
            var clientOptions = new ClientUrlOptions
            {
                ActivateAccountUrl = "http://fake.com",
                ResetPasswordUrl = "http://fake.com"
            };
            _options = Options.Create(clientOptions);
        }

        private void CreateHandler(
            IUserRepository userRepo,
            IOTPGeneratorService generatorService,
            IEmailBackgroundQueue emailQueue,
            ICacheService cacheService,
            IPasswordHashingService hashingService
        )
        {
            _handler = new CreateUserCommandHandler(
                _logger.Object,
                userRepo,
                generatorService,
                emailQueue,
                cacheService,
                _options,
                hashingService
            );
        }

        [Fact]
        public async Task Handler_Should_ReturnUserId_WhenAllParametersAreValid()
        {
            // Arrange
            const string username = "lynx_user";
            const string password = "VeryStrong@Password123";
            const string passwordHash = "VeryStrong@Hash123";
            const string email = "user@lynx.com";
            const string idempotencyKey = "VeryRandomIdempotencyKey123";
            const string urlSafeToken = "VeryUrlSafeToken";
            CreateUserCommand request = new()
            {
                Email = email,
                IdempotencyKey = idempotencyKey,
                Password = password,
                Username = username
            };
            var userRepoMock = new UserRepositoryMockBuilder()
                .WithUniqueEmail(email)
                .WithUniqueUsername(username)
                .WithSuccessfulDatabaseAdd();
            var otpMock = new OtpGeneratorServiceMockBuilder().WithGenerateUrlSafeToken(urlSafeToken);
            var emailMock = new EmailBackgroundQueueMockBuilder().WithSuccessfulQueueEmail();
            var cacheMock = new CacheServiceMockBuilder().WithSuccessfulSet<Guid>();
            var passwordHashMock = new PasswordHashingServiceMockBuilder().WithSuccessfulHash(passwordHash, password);
            CreateHandler(
                userRepoMock.Object,
                otpMock.Object,
                emailMock.Object,
                cacheMock.Object,
                passwordHashMock.Object
            );

            // Act
            var result = await _handler!.Handle(request, default);

            // Aseert
            result.IsSuccess.Should().BeTrue();
            var id = result.Value;
            userRepoMock.Mock.Verify(repo => repo.IsEmailUniqueAsync(email, It.IsAny<CancellationToken>()), Times.Once());
            userRepoMock.Mock.Verify(repo => repo.IsUsernameUniqueAsync(username, It.IsAny<CancellationToken>()), Times.Once());
            otpMock.Mock.Verify(service => service.GenerateUrlSafeToken(It.IsAny<int>()), Times.Once());
            emailMock.Mock.Verify(service => service.QueueEmailAsync(
                It.IsAny<EmailJob>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());
            cacheMock.Mock.Verify(service => service.SetAsync(
                $"activation-codes:{urlSafeToken}",
                id,
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ), Times.Once());
            passwordHashMock.Mock.Verify(service => service.Hash(password), Times.Once());
            userRepoMock.Mock.Verify(repo => repo.AddAsync(It.Is<User>(user =>
                user.Email == email &&
                user.Password == passwordHash &&
                user.Username == username
            ), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnEmailAlreadyExists_WhenEmailIsNotUnique()
        {
            // Arrange
            const string username = "lynx_user";
            const string password = "VeryStrong@Password123";
            const string email = "user@lynx.com";
            const string idempotencyKey = "VeryRandomIdempotencyKey123";
            CreateUserCommand request = new()
            {
                Email = email,
                IdempotencyKey = idempotencyKey,
                Password = password,
                Username = username
            };
            var userRepoMock = new UserRepositoryMockBuilder()
                .WithDuplicateEmail(email)
                .WithUniqueUsername(username);
            var otpMock = new OtpGeneratorServiceMockBuilder();
            var cacheMock = new CacheServiceMockBuilder();
            var emailMock = new EmailBackgroundQueueMockBuilder();
            var passwordHashMock = new PasswordHashingServiceMockBuilder();
            CreateHandler(
                userRepoMock.Object,
                otpMock.Object,
                emailMock.Object,
                cacheMock.Object,
                passwordHashMock.Object
            );

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.EmailAlreadyExists.Code);
        }

        [Fact]
        public async Task Handler_Should_ReturnUsernameAlreadyExists_WhenUsernameIsNotUnique()
        {
            // Arrange
            const string username = "lynx_user";
            const string password = "VeryStrong@Password123";
            const string email = "user@lynx.com";
            const string idempotencyKey = "VeryRandomIdempotencyKey123";
            CreateUserCommand request = new()
            {
                Email = email,
                IdempotencyKey = idempotencyKey,
                Password = password,
                Username = username
            };
            var userRepoMock = new UserRepositoryMockBuilder()
                .WithUniqueEmail(email)
                .WithDuplicateUsername(username);
            var otpMock = new OtpGeneratorServiceMockBuilder().WithGenerateUrlSafeToken("");
            var cacheMock = new CacheServiceMockBuilder();
            var emailMock = new EmailBackgroundQueueMockBuilder();
            var passwordHashMock = new PasswordHashingServiceMockBuilder().WithSuccessfulHash("");
            CreateHandler(
                userRepoMock.Object,
                otpMock.Object,
                emailMock.Object,
                cacheMock.Object,
                passwordHashMock.Object
            );

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UsernameAlreadyExists.Code);
        }

        [Fact]
        public async Task Handler_Should_ReturnErrors_WhenUserCreateReturnsAnError()
        {
            // Arrange
            const string username = "";
            const string password = "VeryStrong@Password123";
            const string email = "user@lynx.com";
            const string idempotencyKey = "VeryRandomIdempotencyKey123";
            CreateUserCommand request = new()
            {
                Email = email,
                IdempotencyKey = idempotencyKey,
                Password = password,
                Username = username
            };
            var userRepoMock = new UserRepositoryMockBuilder()
                .WithUniqueEmail(email)
                .WithUniqueUsername(username);
            var otpMock = new OtpGeneratorServiceMockBuilder().WithGenerateUrlSafeToken("");
            var cacheMock = new CacheServiceMockBuilder();
            var emailMock = new EmailBackgroundQueueMockBuilder();
            var passwordHashMock = new PasswordHashingServiceMockBuilder().WithSuccessfulHash("");
            CreateHandler(
                userRepoMock.Object,
                otpMock.Object,
                emailMock.Object,
                cacheMock.Object,
                passwordHashMock.Object
            );

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(UserErrors.UsernameRequired.Code);
        }
    }
}
