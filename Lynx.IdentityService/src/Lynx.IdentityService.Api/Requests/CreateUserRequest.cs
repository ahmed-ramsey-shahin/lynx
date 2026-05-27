namespace Lynx.IdentityService.Api.Requests
{
    public sealed record CreateUserRequest
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
        public string Email { get; init; } = null!;
    }
}
