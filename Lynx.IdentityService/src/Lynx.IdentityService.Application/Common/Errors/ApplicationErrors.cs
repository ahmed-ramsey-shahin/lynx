using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Application.Common.Errors
{
    public static class ApplicationErrors
    {
        public static Error EmailAlreadyExists => Error.Conflict("User.EmailConflict", "A user with this email already exists.");
        public static Error UsernameAlreadyExists => Error.Conflict("User.UsernameConflict", "A user with this username already exists.");
        public static Error EmailInvalid => Error.Validation("User.InvalidEmail", "The provided email address is invalid.");
        public static Error UsernameInvalid => Error.Validation("User.InvalidUsername", "Invalid username. Use 3-16 characters, starting with a letter, and only letters, numbers, or underscores.");
        public static Error PasswordInvalid => Error.Validation("User.InvalidPasswordFormat", "The password must be at least 8 characters long and include an uppercase letter, a lowercase letter, a number, and a special character.");
    }
}
