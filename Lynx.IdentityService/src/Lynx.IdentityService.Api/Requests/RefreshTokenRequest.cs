namespace Lynx.IdentityService.Api.Requests
{
    public record RefreshTokenRequest
    {
        public string RefreshToken { get; init; } = default!;
        public string ExpiredAccessToken { get; init; } = default!;
    }
}
