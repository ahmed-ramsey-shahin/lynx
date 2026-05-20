using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Domain.Identity
{
    public static class UserErrors
    {
        public static Error IdRequired => Error.Validation("User.IdRequired", "User ID is required.");
        public static Error PasswordRequired => Error.Validation("User.PasswordRequired", "Password is required.");
        public static Error EmailRequired => Error.Validation("User.EmailRequired", "Email address is required.");
        public static Error UsernameRequired => Error.Validation("User.UsernameRequired", "Username address is required.");
        public static Error NotActivated => Error.Forbidden("User.NotActivated", "This operation cannot be performed because the user is not activated yet.");
        public static Error AlreadyActivated => Error.Forbidden("User.AlreadyActivated", "This operation cannot be performed because the user is already activated.");
    }
}
