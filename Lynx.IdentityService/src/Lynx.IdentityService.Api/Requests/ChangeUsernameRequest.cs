namespace Lynx.IdentityService.Api.Requests
{
    public sealed record ChangeUsernameRequest
    {
        public string NewUsername { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
