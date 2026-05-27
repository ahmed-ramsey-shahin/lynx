namespace Lynx.IdentityService.Api.Requests
{
    public sealed record ChangeUsernameRequest
    {
        public Guid UserId { get; init; }
        public string NewUsername { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
