using Lynx.IdentityService.Domain.Common;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed record UserRegisteredEvent(Guid UserId, string Email, string Username) : DomainEvent;
    public sealed record PasswordChangedEvent(Guid UserId, string Email, string Username) : DomainEvent;
    public sealed record UsernameChangedEvent(Guid UserId, string Email, string OldUsername, string NewUsername) : DomainEvent;
    public sealed record UserActivatedEvent(Guid UserId, string Email, string Username) : DomainEvent;
    public sealed record UserDeletedEvent(Guid UserId, string Email, string Username) : DomainEvent;
}
