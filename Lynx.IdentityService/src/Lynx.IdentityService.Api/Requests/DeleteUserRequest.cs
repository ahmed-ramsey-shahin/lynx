namespace Lynx.IdentityService.Api.Requests
{
    public sealed record DeleteUserRequest
    {
        public Guid UserId { get; init; }
        public string Password  { get; init; } = null!;
        public bool HasConfirmed { get; init; }
    }
}
