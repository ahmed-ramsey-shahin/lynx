namespace Lynx.IdentityService.Application.Features.Identity.Dtos
{
    public sealed record UserDto
    {
        public string UserId { get; init; } = null!;
        public string Username { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string PasswordHash { get; init; } = null!;
    }
}
