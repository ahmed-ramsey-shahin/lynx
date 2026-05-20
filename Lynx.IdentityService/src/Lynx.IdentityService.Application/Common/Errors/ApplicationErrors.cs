using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Application.Common.Errors
{
    public static class ApplicationErrors
    {
        public static Error EmailAlreadyExists => Error.Conflict("User.EmailConflict", "A user with this email already exists.");
        public static Error UsernameAlreadyExists => Error.Conflict("User.UsernameConflict", "A user with this username already exists.");
    }
}
