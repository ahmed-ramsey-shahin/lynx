namespace Lynx.IdentityService.Api.Requests
{
    public sealed record RequestPasswordResetRequest
    {
        public string Email { get; init; } = null!;
    }
}
