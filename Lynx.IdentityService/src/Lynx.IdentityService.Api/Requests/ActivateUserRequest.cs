namespace Lynx.IdentityService.Api.Requests
{
    public sealed record ActivateUserRequest
    {
        public string ActivationCode { get; init; } = null!;
    }
}
