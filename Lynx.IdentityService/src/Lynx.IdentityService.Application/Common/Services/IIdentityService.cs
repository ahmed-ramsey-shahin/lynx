using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Application.Common.Services
{
    public interface IIdentityService
    {
        Task<Result<UserDto>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken=default);
        Task<Result<UserDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken=default);
    }
}
