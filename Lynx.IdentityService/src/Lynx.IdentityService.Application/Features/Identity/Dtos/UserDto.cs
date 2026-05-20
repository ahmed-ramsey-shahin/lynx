namespace Lynx.IdentityService.Application.Features.Identity.Dtos
{
    public sealed record UserDto
    {
        public Guid UserId { get; init; }
        public string Username { get; init; } = null!;
        public string Email { get; init; } = null!;
    }
}
