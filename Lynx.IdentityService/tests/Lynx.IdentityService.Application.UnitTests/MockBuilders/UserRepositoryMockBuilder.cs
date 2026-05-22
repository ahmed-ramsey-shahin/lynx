using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Domain.Identity;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class UserRepositoryMockBuilder
    {
        public Mock<IUserRepository> Mock { get; } = new(MockBehavior.Strict);
        public IUserRepository Object => Mock.Object;

        public UserRepositoryMockBuilder WithUniqueEmail(string email)
        {
            Mock.Setup(repo => repo.IsEmailUniqueAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return this;
        }

        public UserRepositoryMockBuilder WithDuplicateEmail(string email)
        {
            Mock.Setup(repo => repo.IsEmailUniqueAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            return this;
        }

        public UserRepositoryMockBuilder WithUniqueUsername(string username)
        {
            Mock.Setup(repo => repo.IsUsernameUniqueAsync(username, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return this;
        }

        public UserRepositoryMockBuilder WithDuplicateUsername(string username)
        {
            Mock.Setup(repo => repo.IsUsernameUniqueAsync(username, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            return this;
        }

        public UserRepositoryMockBuilder WithSuccessfulDatabaseAdd()
        {
            Mock.Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return this;
        }

        public UserRepositoryMockBuilder WithSuccessfulDatabaseUpdate()
        {
            Mock.Setup(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return this;
        }

        public UserRepositoryMockBuilder WithSuccessfulDeleteUnactivatedUsers(DateTimeOffset? maxCreationDate, int result=0)
        {
            if (maxCreationDate is null)
            {
                Mock.Setup(repo => repo.DeleteUnactivatedUsersAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
            }
            else
            {
                Mock.Setup(repo => repo.DeleteUnactivatedUsersAsync(maxCreationDate.Value, It.IsAny<CancellationToken>())).ReturnsAsync(result);
            }
            return this;
        }

        public UserRepositoryMockBuilder WithUserById(Guid? id=null, User? user=null)
        {
            if (id is null)
            {
                Mock.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
            }
            else
            {
                Mock.Setup(repo => repo.GetUserByIdAsync(id.Value, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
            }

            return this;
        }

        public UserRepositoryMockBuilder WithUserByUsername(string? username=null, User? user=null)
        {
            if (username is null)
            {
                Mock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
            }
            else
            {
                Mock.Setup(repo => repo.GetUserByUsernameAsync(username, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
            }

            return this;
        }

        public UserRepositoryMockBuilder WithUserByEmail(string? email=null, User? user=null)
        {
            if (email is null)
            {
                Mock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
            }
            else
            {
                Mock.Setup(repo => repo.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
            }

            return this;
        }
    }
}
