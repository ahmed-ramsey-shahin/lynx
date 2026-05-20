namespace Lynx.IdentityService.Domain.Common
{
    public abstract class EventfulEntity(TimeProvider provider) : AuditableEntity(provider)
    {
        private readonly List<DomainEvent> _events = [];
        public IReadOnlyCollection<DomainEvent> Events => _events;

        public void AddEvent(DomainEvent @event)
        {
            _events.Add(@event);
        }

        public void ClearEvents()
        {
            _events.Clear();
        }

        public void RemoveEvent(DomainEvent @event)
        {
            _events.Remove(@event);
        }
    }
}
