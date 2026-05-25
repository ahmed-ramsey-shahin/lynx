namespace Lynx.IdentityService.Infrastructure.Settings
{
    public sealed record EmailSettings
    {
        public string SenderEmail { get; init; } = null!;
        public string SenderName { get; init; } = null!;
    }
}
