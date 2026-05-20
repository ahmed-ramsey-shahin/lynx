using Lynx.IdentityService.Domain.Identity;

namespace Lynx.IdentityService.Application.Common.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken=default);
        Task<bool> IsUsernameUniqueAsync(string email, CancellationToken cancellationToken=default);
        Task<bool> AddAsync(User user, CancellationToken cancellationToken=default);
    }
}
