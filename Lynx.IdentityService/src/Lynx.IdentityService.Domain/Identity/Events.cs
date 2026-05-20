using Lynx.IdentityService.Domain.Common;

namespace Lynx.IdentityService.Domain.Identity
{
    public sealed record UserRegistered(string Username) : DomainEvent;
}
