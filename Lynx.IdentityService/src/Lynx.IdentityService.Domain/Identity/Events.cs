using Lynx.IdentityService.Domain.Common;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed record UserRegisteredEvent(Guid UserId) : DomainEvent;
    public sealed record PasswordChangedEvent(Guid UserId) : DomainEvent;
    public sealed record UsernameChangedEvent(Guid UserId) : DomainEvent;
    public sealed record UserActivatedEvent(Guid UserId) : DomainEvent;
    public sealed record UserDeletedEvent(Guid UserId) : DomainEvent;
}
