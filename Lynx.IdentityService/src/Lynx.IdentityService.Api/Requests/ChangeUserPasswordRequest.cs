namespace Lynx.IdentityService.Api.Requests
{
    public sealed record ChangeUserPasswordRequest
    {
        public Guid UserId { get; init; }
        public string NewPassword { get; init; } = null!;
        public string OldPassword { get; init; } = null!;
    }
}
