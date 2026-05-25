using FluentAssertions;
using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Domain.Identity;
using Lynx.IdentityService.Infrastructure.Data;
using Lynx.IdentityService.Infrastructure.Tests.Fixtures;
using MongoDB.Driver;

namespace Lynx.IdentityService.Infrastructure.Tests
{
    [Collection("DatabaseCollection")]
    public class UserRepositoryTests
    {
        private readonly IMongoDatabase _database;
        private readonly IUserRepository _userRepository;

        public UserRepositoryTests(DatabaseFixture fixture)
        {
            _userRepository = new UserRepository(fixture.MongoClient);
            _database = fixture.MongoClient.GetDatabase(DbConstants.DbName);
        }

        [Fact]
        public async Task AddAsync_Should_InsertUserSuccessfully()
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
        }
    }
}
