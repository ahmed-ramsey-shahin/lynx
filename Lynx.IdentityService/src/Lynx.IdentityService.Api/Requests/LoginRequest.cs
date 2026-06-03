namespace Lynx.IdentityService.Api.Requests
{
    public record LoginRequest
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
