using FluentAssertions;
using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Common.Settings;
using Lynx.IdentityService.Application.Features.Identity.Commands.RequestPasswordReset;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class RequestPasswordResetCommandTests
    {
        private readonly Mock<ILogger<RequestPasswordResetCommandHandler>> _logger = new(MockBehavior.Loose);
        private RequestPasswordResetCommandHandler? _handler;

        private void CreateHandler(
            IUserRepository userRepo,
            ICacheService cacheService,
            IEmailBackgroundQueue emailQueue,
            IOTPGeneratorService otpService
        )
        {
            var clientUrls = new ClientUrlOptions
            {
                ActivateAccountUrl = "http://fake.com",
                ResetPasswordUrl = "http://fake.com"
            };
            _handler = new(
                userRepo,
                _logger.Object,
                cacheService,
                emailQueue,
                otpService,
                Options.Create(clientUrls)
            );
        }

        [Fact]
        public async Task Handler_Should_ReturnUpdated_WhenRequestIsValid()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            const string username = "lynx_user";
            const string email = "user@lynx.com";
            const string passwordHash = "VeryStrongHash";
            const string idempotencyKey = "VeryGoodIdempotencyKeyWhichDescripesTheCurrentOperation";
            const string otp = "132465";
            string cacheKey = $"reset_password_otps:{userId}";
            var request = new RequestPasswordResetCommand()
            {
                Email = email,
                IdempotencyKey = idempotencyKey
            };
            var user = User.Create(userId, email, username, passwordHash).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var userRepo = new UserRepositoryMockBuilder().WithUserByEmail(email, user);
            var cacheService = new CacheServiceMockBuilder().WithSuccessfulSet<string>(cacheKey, otp);
            var emailQueue = new EmailBackgroundQueueMockBuilder().WithSuccessfulQueueEmail();
            var otpService = new OtpGeneratorServiceMockBuilder().WithGenerateResetCode(otp);
            CreateHandler(userRepo.Object, cacheService.Object, emailQueue.Object, otpService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            userRepo.Mock.Verify(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once());
            otpService.Mock.Verify(service => service.GenerateResetCode(It.IsAny<int>()), Times.Once());
            cacheService.Mock.Verify(service => service.SetAsync(cacheKey, otp, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once());
            emailQueue.Mock.Verify(service => service.QueueEmailAsync(
                It.Is<EmailJob>(job =>
                    job.To == email &&
                    job.Username == username
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }

        [Fact]
        public async Task Handler_Should_ReturnSuccess_WhenEmailIsNotCorrect()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            const string email = "user@lynx.com";
            const string idempotencyKey = "VeryGoodIdempotencyKeyWhichDescripesTheCurrentOperation";
            const string otp = "132465";
            string cacheKey = $"reset_password_otps:{userId}";
            var request = new RequestPasswordResetCommand()
            {
                Email = email,
                IdempotencyKey = idempotencyKey
            };
            var userRepo = new UserRepositoryMockBuilder().WithUserByEmail(email, null);
            var cacheService = new CacheServiceMockBuilder();
            var emailService = new EmailBackgroundQueueMockBuilder();
            var otpService = new OtpGeneratorServiceMockBuilder().WithGenerateResetCode(otp);
            CreateHandler(userRepo.Object, cacheService.Object, emailService.Object, otpService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            userRepo.Mock.Verify(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
