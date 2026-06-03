using FluentAssertions;
using Lynx.IdentityService.Application.Common.Errors;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Queries.GetUser;
using Lynx.IdentityService.Application.UnitTests.MockBuilders;
using Lynx.IdentityService.Domain.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests
{
    public class GetUserQueryTests
    {
        private readonly Mock<ILogger<GetUserQueryHandler>> _logger = new(MockBehavior.Loose);
        private GetUserQueryHandler? _handler;

        private void CreateHandler(IUserRepository userRepository, IUserService userService)
        {
            _handler = new(_logger.Object, userRepository, userService);
        }

        [Fact]
        public async Task Handler_Should_ReturnCorrectUser_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string username = "awesome_username";
            var user = User.Create(
                userId,
                "email@lynx.com",
                username,
                "StrongPasswordHash"
            ).Value;
            user.Activate(DateTimeOffset.UtcNow);
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, user);
            var userService = new UserServiceMockBuilder().WithId(userId);
            var request = new GetUserQuery()
            {
                UserId = userId.ToString()
            };
            CreateHandler(userRepo.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Username.Should().Be(username);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
            userService.Mock.Verify(service => service.UserId, Times.AtLeastOnce());
        }

        [Fact]
        public async Task Handler_Should_ReturnUserNotFound_WhenUsernameIsNotCorrect()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string username = "awesome_username";
            var userRepo = new UserRepositoryMockBuilder().WithUserById(userId, null);
            var userService = new UserServiceMockBuilder().WithId(userId);
            var request = new GetUserQuery()
            {
                UserId = userId.ToString()
            };
            CreateHandler(userRepo.Object, userService.Object);

            // Act
            var result = await _handler!.Handle(request, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Code.Should().Be(ApplicationErrors.UserNotFound.Code);
            userRepo.Mock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
