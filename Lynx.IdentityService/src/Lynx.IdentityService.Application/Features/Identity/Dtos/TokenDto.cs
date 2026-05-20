namespace Lynx.IdentityService.Application.Features.Identity.Dtos
{
    public sealed record TokenDto
    {
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
