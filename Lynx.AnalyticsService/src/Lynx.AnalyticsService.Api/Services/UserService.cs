using System.Security.Claims;

namespace Lynx.AnalyticsService.Api.Services
{
    public interface IUserService
    {
        Guid? UserId { get; }
    }

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
