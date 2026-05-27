using FluentAssertions;
using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.IdentityService.Infrastructure.Tests
{
    public class EmailBackgroundWorkerTests
    {
        private readonly IEmailBackgroundQueue _emailQueue = new EmailBackgroundQueue();
        private readonly Mock<IEmailService> _emailServiceMock = new(MockBehavior.Strict);
        private readonly Mock<ILogger<EmailBackgroundWorker>> _loggerMock = new(MockBehavior.Loose);
        private readonly EmailBackgroundWorker _emailWorker;

        public EmailBackgroundWorkerTests()
        {
            _emailServiceMock.Setup(service => service.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
            _emailWorker = new(_emailQueue, _emailServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Should_SendEmail_WhenThereAreDataInTheQueue()
        {
            // Arrange
            var job = new EmailJob("To", "Username", "Subject", "Body");
            await _emailQueue.QueueEmailAsync(job, default);
            using var cts = new CancellationTokenSource();

            // Act
            await _emailWorker.StartAsync(cts.Token);
            await Task.Delay(50);
            await cts.CancelAsync();
            await _emailWorker.StopAsync(cts.Token);

            // Assert
            _emailServiceMock.Verify(service => service.SendEmailAsync(
                job.To,
                job.Username,
                job.Subject,
                job.Body,
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Should_SurviveExceptions()
        {
            // Arrange
            var badJob = new EmailJob("Bad", "Username", "Subject", "Body");
            var goodJob = new EmailJob("Good", "Username", "Subject", "Body");
            await _emailQueue.QueueEmailAsync(badJob, default);
            await _emailQueue.QueueEmailAsync(goodJob, default);
            using var cts = new CancellationTokenSource();
            _emailServiceMock.Setup(service => service.SendEmailAsync(
                badJob.To,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).Throws(new Exception("RandomException"));

            // Act
            await _emailWorker.StartAsync(cts.Token);
            await Task.Delay(50);
            await cts.CancelAsync();
            await _emailWorker.StopAsync(cts.Token);

            // Assert
            _emailServiceMock.Verify(service => service.SendEmailAsync(
                badJob.To,
                badJob.Username,
                badJob.Subject,
                badJob.Body,
                It.IsAny<CancellationToken>()
            ), Times.Once());
            _emailServiceMock.Verify(service => service.SendEmailAsync(
                goodJob.To,
                goodJob.Username,
                goodJob.Subject,
                goodJob.Body,
                It.IsAny<CancellationToken>()
            ), Times.Once());
        }
    }
}
