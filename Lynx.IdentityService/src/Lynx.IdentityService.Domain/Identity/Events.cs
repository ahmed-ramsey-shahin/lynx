using Lynx.IdentityService.Domain.Common;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed record UserRegistered(Guid UserId) : DomainEvent;
    public sealed record PasswordChanged(Guid UserId) : DomainEvent;
    public sealed record UsernameChanged(Guid UserId) : DomainEvent;
    public sealed record UserActivated(Guid UserId) : DomainEvent;
    public sealed record UserDeleted(Guid UserId) : DomainEvent;
}
