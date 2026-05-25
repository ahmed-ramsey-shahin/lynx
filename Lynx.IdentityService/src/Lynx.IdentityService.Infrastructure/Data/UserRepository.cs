using Lynx.IdentityService.Application.Common.Repositories;
using Lynx.IdentityService.Domain.Identity;
using MongoDB.Driver;

namespace Lynx.IdentityService.Infrastructure.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoClient _client;

        public UserRepository(IMongoClient client)
        {
            _client = client;
            var database = _client.GetDatabase(DbConstants.DbName);
            _users = database.GetCollection<User>(DbConstants.UserTableName);
        }

        public async Task<bool> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _users.InsertOneAsync(user, null, cancellationToken);
            return true;
        }

        public async Task<int> DeleteUnactivatedUsersAsync(DateTimeOffset maxCreationDate, CancellationToken cancellationToken = default)
        {
            var notActivatedFilter = Builders<User>.Filter.Eq(user => user.IsActivated, false);
            var activationDatePassedFilter = Builders<User>.Filter.Lte(user => user.CreatedAt, maxCreationDate);
            var filter = Builders<User>.Filter.And([notActivatedFilter, activationDatePassedFilter]);
            var result = await _users.DeleteManyAsync(filter, cancellationToken);
            return (int) result.DeletedCount;
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _users.Find(user => user.Username == username).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            var filter = Builders<User>.Filter.Eq(user => user.Email, email);
            return await _users.CountDocumentsAsync(
                filter,
                new CountOptions
                {
                    Limit = 1,
                    Hint = DbConstants.EmailIndexName
                },
                cancellationToken
            ) == 0;
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default)
        {
            var filter = Builders<User>.Filter.Eq(user => user.Username, username);
            return await _users.CountDocumentsAsync(
                filter,
                new CountOptions
                {
                    Limit = 1,
                    Hint = DbConstants.UsernameIndexName
                },
                cancellationToken
            ) == 0;
        }

        public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var result = await _users.ReplaceOneAsync(
                u => u.Id == user.Id,
                user,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken
            );
            return result.IsAcknowledged && result.MatchedCount > 0;
        }
    }
}
