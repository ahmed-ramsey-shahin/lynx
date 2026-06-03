using System.Security.Claims;
using Lynx.IdentityService.Application.Common.Services;

namespace Lynx.IdentityService.Api.Services
{
    public class UserService(IHttpContextAccessor contextAccessor) : IUserService
    {
        public Guid? UserId
        {
            get
            {
                var claimsPrincipal = contextAccessor.HttpContext?.User;
                var userIdString = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdString, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }
    }
}
