namespace Lynx.IdentityService.Infrastructure.Configurations
{
    public sealed record EmailServiceConfigurations
    {
        public string SenderEmail { get; init; } = null!;
        public string SenderName { get; init; } = null!;
    }
}
