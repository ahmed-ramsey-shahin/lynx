using Lynx.IdentityService.Domain.Common;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed record UserRegistered(string UserId) : DomainEvent;
    public sealed record PasswordChanged(string UserId) : DomainEvent;
    public sealed record UsernameChanged(string UserId) : DomainEvent;
    public sealed record UserActivated(string UserId) : DomainEvent;
    public sealed record UserDeleted(string UserId) : DomainEvent;
}
