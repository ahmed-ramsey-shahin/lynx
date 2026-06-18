using Lynx.RedirectionService.Domain.Common;

namespace Lynx.RedirectionService.Domain.Urls
{
    public sealed record UrlVisitedEvent(Guid UrlId) : DomainEvent;
}
