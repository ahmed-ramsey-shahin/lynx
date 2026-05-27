namespace Lynx.IdentityService.Api.Requests
{
    public sealed record ChangeUserPasswordRequest
    {
        public string NewPassword { get; init; } = null!;
        public string OldPassword { get; init; } = null!;
    }
}
