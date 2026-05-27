using Lynx.IdentityService.Application.Common.BackgroundJobs;
using Lynx.IdentityService.Application.Features.Identity.EventHandlers;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Contracts;
using Lynx.IdentityService.Domain.Common;
using Lynx.IdentityService.Domain.Identity;
using MediatR;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class EmailEventHandlerTests
    {
        private static async Task VerifyEmailQueue<TEvent>(
            TEvent domainEvent,
            Func<IEmailBackgroundQueue, INotificationHandler<TEvent>> handlerFactory,
            EmailJob expectedJob
        ) where TEvent : DomainEvent
        {
            // Arrange
            var queueMock = new EmailBackgroundQueueMockBuilder().WithSuccessfulQueueEmail();
            var handler = handlerFactory(queueMock.Object);

            // Act
            await handler.Handle(domainEvent, default);

            // Assert
            queueMock.Mock.Verify(q => q.QueueEmailAsync(expectedJob, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UserRegistered_Should_QueueEmail()
        {
            const string email = "test@lynx.com";
            const string username = "lynx_user";
            var evt = new UserRegisteredEvent(
                Guid.NewGuid(),
                email,
                username
            );
            var expectedJob = new EmailJob(
                email,
                username,
                "Welcome to Lynx",
                $"Hi {username}, welcome to Lynx."
            );
            await VerifyEmailQueue(evt, q => new UserRegisteredEventHandler(q), expectedJob);
        }

        [Fact]
        public async Task UserActivated_Should_QueueEmail()
        {
            const string email = "test@lynx.com";
            const string username = "lynx_user";
            var evt = new UserActivatedEvent(
                Guid.NewGuid(),
                email,
                username
            );
            var expectedJob = new EmailJob(
                email,
                username,
                "Account activation",
                $"Hi {username}, you account has been successfully activated."
            );
            await VerifyEmailQueue(evt, q => new UserActivatedEventHandler(q), expectedJob);
        }

        [Fact]
        public async Task UserDeleted_Should_QueueEmail()
        {
            const string email = "test@lynx.com";
            const string username = "lynx_user";
            var evt = new UserDeletedEvent(
                Guid.NewGuid(),
                email,
                username
            );
            var expectedJob = new EmailJob(
                email,
                username,
                "Account Deleted",
            $"Hi {username}, your account has been deleted. We are sorry you had to leave us."
            );
            await VerifyEmailQueue(evt, q => new UserDeletedEventHandler(q), expectedJob);
        }

        [Fact]
        public async Task UsernameChanged_Should_QueueEmail()
        {
            const string email = "test@lynx.com";
            const string oldUsername = "lynx_user";
            const string newUsername = "new_lynx_user";
            var evt = new UsernameChangedEvent(
                Guid.NewGuid(),
                email,
                oldUsername,
                newUsername
            );
            var expectedJob = new EmailJob(
                email,
                newUsername,
                "Your username has changed",
                @$"Hi {newUsername}, your username has changed from {oldUsername} to {newUsername}.
If you did not attempt to change it, please contact the customer support ASAP."
            );
            await VerifyEmailQueue(evt, q => new UsernameChangedEventHandler(q), expectedJob);
        }

        [Fact]
        public async Task PasswordChanged_Should_QueueEmail()
        {
            const string email = "test@lynx.com";
            const string username = "lynx_user";
            var evt = new PasswordChangedEvent(
                Guid.NewGuid(),
                email,
                username
            );
            var expectedJob = new EmailJob(
                email,
                username,
                "Your password has changed",
                @$"Hi {username} your password has changed.
If you did not attempt to change it, please contact the customer support ASAP."
            );
            await VerifyEmailQueue(evt, q => new PasswordChangedEventHandler(q), expectedJob);
        }
    }
}
