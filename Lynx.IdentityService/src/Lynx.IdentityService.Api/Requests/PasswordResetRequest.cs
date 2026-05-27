namespace Lynx.IdentityService.Api.Requests
{
    public sealed record PasswordResetRequest
    {
        public string Email { get; init; } = null!;
        public string Code { get; init; } = null!;
        public string NewPassword { get; init; } = null!;
    }
}
