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
        public static Error UserNotFound => Error.NotFound("User.NotFound", "The requested user was not found.");
        public static Error InvalidOldPassword => Error.Validation("User.InvalidOldPassword", "The provided current password does not match the actual password.");
        public static Error ExpiredAccessTokenInvalid => Error.Unauthorized("Auth.InvalidExpiredToken", "The provided expired access token is invalid or malformed.");
        public static Error UsernameClaimInvalid => Error.Unauthorized("Auth.InvalidUsernameClaim", "The username claim in the token is invalid or missing.");
        public static Error RefreshTokenExpired => Error.Unauthorized("Auth.RefreshTokenExpired", "The refresh token has expired. Please log in again.");
        public static Error CredentialsInvalid => Error.Unauthorized("Auth.InvalidCredentials", "The provided email or password is incorrect.");
        public static Error UserNotActive => Error.Forbidden("User.NotActive", "This operation cannot be performed because the user is not active.");
        public static Error OtpInvalid => Error.Validation("User.InvalidOtp", "The OTP must be 6 digits.");
        public static Error ActivationCodeInvalid => Error.Validation("User.InvalidActivationCode", "The activation code must be 64 characters.");
        public static Error OtpExpired => Error.Forbidden("User.OtpExpired", "The provided OTP is invalid or expired.");
        public static Error ActivationCodeExpired => Error.Forbidden("User.ActivationCodeExpired", "The provided activation code is invalid or expired.");
        public static Error DeletionNotConfirmed => Error.Forbidden("User.DeletionNotConfirmed", "You must explicitly confirm the deletion to permanently remove all your links and associated analytics.");
    }
}
