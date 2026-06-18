namespace Lynx.RedirectionService.Domain.Common
{
    public abstract class AuditableEntity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
