namespace Lynx.IdentityService.Application.Features.Identity.Dtos
{
    public sealed record TokenDto
    {
        public string AccessToken { get; init; } = null!;
        public string RefreshToken { get; init; } = null!;
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
