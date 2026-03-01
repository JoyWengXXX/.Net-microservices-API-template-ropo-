using CQRS.Core.Events;

namespace CQRS.Core.Infrastructure
{
    public interface IEventStore
    {
        Task<TResult> SaveEventsAsync(IEnumerable<BaseEvent> events, int expectedVersion, bool? IsBatchLastLoop = null);
        Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId);
        Task<List<Guid>> GetAggregateIdsAsync();
        Task UpdateEventStatusAsync(EventModel @event);
    }
}

