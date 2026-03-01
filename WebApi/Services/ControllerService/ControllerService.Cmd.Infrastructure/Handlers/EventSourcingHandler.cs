using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using ControllerService.Cmd.Domain.Aggregates;
using ControllerService.Cmd.Domain.Handlers;
using Microsoft.AspNetCore.Http;

namespace ControllerService.Cmd.Infrastructure.Handlers
{
    public class EventSourcingHandler : IEventSourcingHandler<ControllerAggregate>
    {
        private readonly IEventStore _eventStore;
        private readonly IEventHandler _eventHandler;
        private IHttpContextAccessor _httpContextAccessor;

        public EventSourcingHandler(IEventStore eventStore,
                                    IEventHandler eventHandler,
                                    IHttpContextAccessor httpContextAccessor)
        {
            _eventStore = eventStore;
            _eventHandler = eventHandler;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ControllerAggregate> GetByIdAsync(Guid aggregateId, bool eventSouringEnabled)
        {
            var aggregate = new ControllerAggregate(_httpContextAccessor);
            if (eventSouringEnabled)
            {
                var events = await _eventStore.GetEventsAsync(aggregateId);

                if (events == null || !events.Any()) return aggregate;

                aggregate.ReplayEvents(events);
                aggregate.Version = events.Select(x => x.Version).Last();
            }
            else
            {
                aggregate.Active = true;
            }

            return aggregate;
        }

        public async Task<TResult> SaveAsync(AggregateRoot aggregate, bool eventSouringEnabled)
        {
            if (eventSouringEnabled)
            {
                var result = await _eventStore.SaveEventsAsync(aggregate.GetUncommittedChanges(), aggregate.Version);
                aggregate.MarkChangesAsCommitted();
                return result;
            }
            else
            {
                foreach (var @event in aggregate.GetUncommittedChanges())
                {
                    // Handle reflecting method
                    var handlerMethod = _eventHandler.GetType().GetMethod("On", [@event.GetType()]);
                    if (handlerMethod == null)
                    {
                        throw new ArgumentNullException(nameof(handlerMethod), "Could not find event handler method!");
                    }
                    var task = handlerMethod.Invoke(_eventHandler, new[] { @event });
                    return await (Task<TResult>)task;
                }
                return default;
            }
        }

        public async Task RepublishEventsAsync(bool eventSouringEnabled)
        {
            var aggregateIds = await _eventStore.GetAggregateIdsAsync();

            if (aggregateIds == null || !aggregateIds.Any()) return;

            foreach (var aggregateId in aggregateIds)
            {
                var aggregate = await GetByIdAsync(aggregateId, eventSouringEnabled);

                if (aggregate == null || !aggregate.Active) continue;

                var events = await _eventStore.GetEventsAsync(aggregateId);

                foreach (var @event in events)
                {
                    //Handle reflecting method
                    var handlerMethod = _eventHandler.GetType().GetMethod("On", [@event.GetType()]);
                    if (handlerMethod == null)
                    {
                        throw new ArgumentNullException(nameof(handlerMethod), "Could not find event handler method!");
                    }
                    var task = handlerMethod.Invoke(_eventHandler, [@event]);
                    await (Task)task;
                }
            }
        }
    }
}

