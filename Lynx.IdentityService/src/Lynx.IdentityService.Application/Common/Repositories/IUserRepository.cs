using Lynx.IdentityService.Domain.Identity;

namespace Lynx.IdentityService.Application.Common.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(Guid Id, CancellationToken cancellationToken=default);
        Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken=default);
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken=default);
        Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken=default);
        Task<bool> AddAsync(User user, CancellationToken cancellationToken=default);
        Task<bool> UpdateAsync(User user, CancellationToken cancellationToken=default);
    }
}
