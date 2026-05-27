using FluentAssertions;
using Lynx.IdentityService.Domain.Common;
using Lynx.IdentityService.Domain.Identity;
using Lynx.IdentityService.Infrastructure.Data;
using Lynx.IdentityService.Infrastructure.Data.Configuration;
using Lynx.IdentityService.Infrastructure.Tests.Fixtures;
using MediatR;
using Microsoft.Extensions.Time.Testing;
using MongoDB.Driver;
using Moq;

namespace Lynx.IdentityService.Infrastructure.Tests
{
    [Collection("DatabaseCollection")]
    public class UserRepositoryTests
    {
        private readonly IMongoDatabase _database;
        private readonly UserRepository _userRepository;
        private readonly FakeTimeProvider _timeProvider;
        private readonly Mock<IPublisher> _publisherMock = new(MockBehavior.Strict);

        public UserRepositoryTests(DatabaseFixture fixture)
        {
            _publisherMock.Setup(mock => mock.Publish(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _timeProvider = new FakeTimeProvider();
            _userRepository = new UserRepository(fixture.MongoClient, _publisherMock.Object);
            _database = fixture.MongoClient.GetDatabase(DbConstants.DbName);
            _database.DropCollection(DbConstants.UserTableName);
            MongoDbIndexConfiguration.ConfigureUniqueIndexesAsync(fixture.MongoClient).GetAwaiter().GetResult();
        }

#region ADD_ASYNC_TESTS
        [Fact]
        public async Task AddAsync_Should_InsertUserSuccessfully_WhenUserIsNotDuplicated()
        {
            // Arrange
            var user = User.Create(
                Guid.NewGuid(),
                "user@lynx.com",
                "lynx_user",
                "VeryStrongPassword@123_Hashed"
            ).Value;

            // Act
            await _userRepository.AddAsync(user);

            // Assert
            var collection = _database.GetCollection<User>(DbConstants.UserTableName);
            var foundInDb = await collection.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
            foundInDb.Should().NotBeNull();
            foundInDb.Username.Should().Be("lynx_user");
            _publisherMock.Verify(mock => mock.Publish(It.Is<DomainEvent>(notificiation =>
                notificiation is UserRegisteredEvent &&
                ((UserRegisteredEvent) notificiation).Email == "user@lynx.com" &&
                ((UserRegisteredEvent) notificiation).Username == "lynx_user"
            ), It.IsAny<CancellationToken>()), Times.Once());
            user.Events.Should().BeEmpty();
        }

        [Fact]
        public async Task AddAsync_Should_ThrowException_WhenUsernameIsNotUnique()
        {
            // Arrange
            var user1 = User.Create(
                Guid.NewGuid(),
                "user@lynx.com",
                "lynx_user",
                "VeryStrongPassword@123_Hashed"
            ).Value;
            var user2 = User.Create(
                Guid.NewGuid(),
                "user2@lynx.com",
                "lynx_user",
                "VeryStrongPassword@123_Hashed"
            ).Value;

            // Act
            await _userRepository.AddAsync(user1);

            // Assert
            Func<Task> act = async () => await _userRepository.AddAsync(user2);
            var exception = await act.Should().ThrowAsync<MongoWriteException>();
            exception.Which.WriteError.Category.Should().Be(ServerErrorCategory.DuplicateKey);
            exception.Which.WriteError.Code.Should().Be(11000);
            user2.Events.Should().HaveCount(1);
            _publisherMock.Verify(mock => mock.Publish(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AddAsync_Should_ThrowException_WhenEmailIsNotUnique()
        {
            // Arrange
            var user1 = User.Create(
                Guid.NewGuid(),
                "user@lynx.com",
                "lynx_user",
                "VeryStrongPassword@123_Hashed"
            ).Value;
            var user2 = User.Create(
                Guid.NewGuid(),
                "user@lynx.com",
                "lynx_user2",
                "VeryStrongPassword@123_Hashed"
            ).Value;

            // Act
            await _userRepository.AddAsync(user1);

            // Assert
            Func<Task> act = async () => await _userRepository.AddAsync(user2);
            var exception = await act.Should().ThrowAsync<MongoWriteException>();
            exception.Which.WriteError.Category.Should().Be(ServerErrorCategory.DuplicateKey);
            exception.Which.WriteError.Code.Should().Be(11000);
            user2.Events.Should().HaveCount(1);
            _publisherMock.Verify(mock => mock.Publish(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()), Times.Never());
        }
#endregion // ADD_ASYNC_TESTS

#region GET_USER_BY_ID_ASYNC
        [Fact]
        public async Task GetUserByIdAsync_Should_ReturnUser_WhenIdIsCorrect()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var user = User.Create(
                userId,
                "user@lynx.com",
                "lynx_user",
                "Password@123_Hashed"
            ).Value;
            await _userRepository.AddAsync(user);

            // Act
            var userResult = await _userRepository.GetUserByIdAsync(userId);

            // Assert
            userResult.Should().NotBeNull();
            userResult.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_ReturnNull_WhenIdIsNotFound()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var user = User.Create(
                userId,
                "user@lynx.com",
                "lynx_user",
                "Password@123_Hashed"
            ).Value;
            await _userRepository.AddAsync(user);

            // Act
            var userResult = await _userRepository.GetUserByIdAsync(Guid.NewGuid());

            // Assert
            userResult.Should().BeNull();
        }
#endregion // GET_USER_BY_ID_TESTS

#region DELETE_UNACTIVATED_USERS_TESTS
        [Fact]
        public async Task DeleteUnactivatedUseres_Should_ReturnNumberOfDeletedDocuments()
        {
            // Arrange
            var utcNow = new DateTimeOffset(2026, 5, 25, 14, 24, 20, TimeSpan.Zero);
            _timeProvider.SetUtcNow(utcNow);
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var user3Id = Guid.NewGuid();
            var user4Id = Guid.NewGuid();
            var user5Id = Guid.NewGuid();
            const string password = "Password@123_Hashed";
            var user1 = User.Create(user1Id, "user1@lynx.com", "lynx_user1", password).Value;
            var user2 = User.Create(user2Id, "user2@lynx.com", "lynx_user2", password).Value;
            var user3 = User.Create(user3Id, "user3@lynx.com", "lynx_user3", password).Value;
            var user4 = User.Create(user4Id, "user4@lynx.com", "lynx_user4", password).Value;
            var user5 = User.Create(user5Id, "user5@lynx.com", "lynx_user5", password).Value;
            user1.Activate(_timeProvider.GetUtcNow().AddDays(-5));
            user2.Activate(_timeProvider.GetUtcNow().AddHours(-3));
            user3.CreatedAt = _timeProvider.GetUtcNow().AddDays(-3);
            user4.CreatedAt = _timeProvider.GetUtcNow().AddHours(-1);
            user5.CreatedAt = _timeProvider.GetUtcNow().AddHours(-2).AddSeconds(-1);
            await _userRepository.AddAsync(user1);
            await _userRepository.AddAsync(user2);
            await _userRepository.AddAsync(user3);
            await _userRepository.AddAsync(user4);
            await _userRepository.AddAsync(user5);

            // Act
            var result = await _userRepository.DeleteUnactivatedUsersAsync(_timeProvider.GetUtcNow().AddHours(-2));

            // Assert
            result.Should().Be(2);
            var collection = _database.GetCollection<User>(DbConstants.UserTableName);
            var foundInDb = await collection.Find(_ => true).ToListAsync();
            var expectedRemainingUsernames = new[] {"lynx_user1", "lynx_user2", "lynx_user4"};
            foundInDb.Select(user => user.Username)
                .Should()
                .BeEquivalentTo(expectedRemainingUsernames);
            _publisherMock.Verify(mock => mock.Publish(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()), Times.Never());
        }
#endregion // DELETE_UNACTIVATED_USERS_TESTS

#region GET_USER_BY_EMAIL_TESTS
        [Fact]
        public async Task GetUserByEmailAsync_Should_ReturnUser_WhenEmailIsCorrect()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var user = User.Create(
                userId,
                "user@lynx.com",
                "lynx_user",
                "Password@123_Hashed"
            ).Value;
            await _userRepository.AddAsync(user);

            // Act
            var userResult = await _userRepository.GetUserByEmailAsync("user@lynx.com");

            // Assert
            userResult.Should().NotBeNull();
            userResult.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserByEmailAsync_Should_ReturnNull_WhenEmailIsNotFound()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var user = User.Create(
                userId,
                "user@lynx.com",
                "lynx_user",
                "Password@123_Hashed"
            ).Value;
            await _userRepository.AddAsync(user);

            // Act
            var userResult = await _userRepository.GetUserByEmailAsync("wrong_user@lynx.com");

            // Assert
            userResult.Should().BeNull();
        }
#endregion // GET_USER_BY_EMAIL_TESTS

#region GET_USER_BY_USERNAME_TESTS
        [Fact]
        public async Task GetUserByUsernameAsync_Should_ReturnUser_WhenUsernameIsCorrect()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var user = User.Create(
                userId,
                "user@lynx.com",
                "lynx_user",
                "Password@123_Hashed"
            ).Value;
            await _userRepository.AddAsync(user);

            // Act
            var userResult = await _userRepository.GetUserByUsernameAsync("lynx_user");

            // Assert
            userResult.Should().NotBeNull();
            userResult.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_Should_ReturnNull_WhenUsernameIsNotFound()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var user = User.Create(
                userId,
                "user@lynx.com",
                "lynx_user",
                "Password@123_Hashed"
            ).Value;
            await _userRepository.AddAsync(user);

            // Act
            var userResult = await _userRepository.GetUserByUsernameAsync("lynx_wrong_user");

            // Assert
            userResult.Should().BeNull();
        }
#endregion // GET_USER_BY_USERNAME_TESTS

#region IS_EMAIL_UNIQUE_TESTS
        [Fact]
        public async Task IsEmailUniqueAsync_Should_ReturnTrue_WhenEmailIsUnique()
        {
            // Arrange
            var user = User.Create(Guid.NewGuid(), "user@lynx.com", "lynx_user", "Password@123_Hashed").Value;
            await _userRepository.AddAsync(user);

            // Act
            var result = await _userRepository.IsEmailUniqueAsync("users2@lynx.com");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsEmailUniqueAsync_Should_ReturnFalse_WhenEmailIsNotUnique()
        {
            // Arrange
            var user = User.Create(Guid.NewGuid(), "user@lynx.com", "lynx_user", "Password@123_Hashed").Value;
            await _userRepository.AddAsync(user);

            // Act
            var result = await _userRepository.IsEmailUniqueAsync("user@lynx.com");

            // Assert
            result.Should().BeFalse();
        }
#endregion // IS_EMAIL_UNIQUE_TESTS

#region IS_USERNAME_UNIQUE_TESTS
        [Fact]
        public async Task IsUsernameUniqueAsync_Should_ReturnTrue_WhenUsernameIsUnique()
        {
            // Arrange
            var user = User.Create(Guid.NewGuid(), "user@lynx.com", "lynx_user", "Password@123_Hashed").Value;
            await _userRepository.AddAsync(user);

            // Act
            var result = await _userRepository.IsUsernameUniqueAsync("lynx_user2");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUsernameUniqueAsync_Should_ReturnFalse_WhenUsernameIsNotUnique()
        {
            // Arrange
            var user = User.Create(Guid.NewGuid(), "user@lynx.com", "lynx_user", "Password@123_Hashed").Value;
            await _userRepository.AddAsync(user);

            // Act
            var result = await _userRepository.IsUsernameUniqueAsync("lynx_user");

            // Assert
            result.Should().BeFalse();
        }
#endregion // IS_EMAIL_UNIQUE_TESTS

#region UPDATE_ASYNC_TESTS
        [Fact]
        public async Task UpdateAsync_Should_UpdateTheUser_WhenParametersAreValid()
        {
            // Arrange
            const string oldUsername = "lynx_user";
            const string newUsername = "new_lynx_user";
            var user = User.Create(
                Guid.NewGuid(),
                "user@lynx.com",
                oldUsername,
                "VeryStrongPassword@123_Hashed"
            ).Value;
            user.Activate(DateTime.UtcNow);
            await _userRepository.AddAsync(user);
            user.ChangeUsername(newUsername);

            // Act
            var result = await _userRepository.UpdateAsync(user, default);

            // Assert
            result.Should().BeTrue();
            var collection = _database.GetCollection<User>(DbConstants.UserTableName);
            var foundInDb = await collection.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
            foundInDb.Should().NotBeNull();
            foundInDb.Username.Should().Be(newUsername);
            _publisherMock.Verify(mock => mock.Publish(It.Is<DomainEvent>(notificiation =>
                notificiation is UsernameChangedEvent &&
                ((UsernameChangedEvent) notificiation).Email == "user@lynx.com" &&
                ((UsernameChangedEvent) notificiation).OldUsername == oldUsername &&
                ((UsernameChangedEvent) notificiation).NewUsername == newUsername
            ), It.IsAny<CancellationToken>()), Times.Once());
            user.Events.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnFalseAndPublishNoEvents_WhenUserDoesNotExist()
        {
            // Arrange
            const string oldUsername = "lynx_user";
            const string newUsername = "new_lynx_user";
            var user = User.Create(
                Guid.NewGuid(),
                "user@lynx.com",
                oldUsername,
                "VeryStrongPassword@123_Hashed"
            ).Value;
            user.Activate(DateTime.UtcNow);
            user.ChangeUsername(newUsername);

            // Act
            var result = await _userRepository.UpdateAsync(user, default);

            // Assert
            result.Should().BeFalse();
            _publisherMock.Verify(mock => mock.Publish(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()), Times.Never());
            user.Events.Should().HaveCount(3);
        }
#endregion // UPDATE_ASYNC_TESTS
    }
}
