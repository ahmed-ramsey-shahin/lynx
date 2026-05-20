namespace Lynx.IdentityService.Domain.Common
{
    public abstract class AuditableEntity(TimeProvider provider)
    {
        public DateTimeOffset CreatedAt { get; set; } = provider.GetUtcNow();
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
