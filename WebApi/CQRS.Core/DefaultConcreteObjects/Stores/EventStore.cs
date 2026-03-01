using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using System.Collections.Concurrent;
using System.Reflection;

namespace CQRS.Core.DefaultConcreteObjects.Stores
{
    public class EventStore<EventHelper> : IEventStore, IDisposable
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly EventHelper _eventHandler;
        private readonly ConcurrentDictionary<Type, MethodInfo> _methodCache;
        private bool _disposed;

        public EventStore(IEventStoreRepository eventStoreRepository, EventHelper eventHandler)
        {
            _eventStoreRepository = eventStoreRepository;
            _eventHandler = eventHandler;
            _methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        }

        private MethodInfo GetHandlerMethod(Type eventType)
        {
            return _methodCache.GetOrAdd(eventType, type =>
            {
                var method = _eventHandler.GetType().GetMethod("On", [type]);
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method),
                        $"Could not find event handler method for type {type.Name}!");
                }
                return method;
            });
        }

        public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);
            if (eventStream == null || !eventStream.Any())
                throw new AggregateNotFoundException($"Incorrect ID provided!");

            return eventStream
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.Version)
                .Select(x => x.EventData)
                .ToList();
        }

        public async Task<TResult> SaveEventsAsync(IEnumerable<BaseEvent> events, int expectedVersion, bool? isBatchLastLoop = null)
        {
            // 使用 ToList() 避免多次枚舉
            var eventsList = events.ToList();

            // 驗證版本
            foreach (var @event in eventsList)
            {
                var eventStream = await _eventStoreRepository.FindByAggregateId(@event.id);
                if (expectedVersion != -1 && eventStream[^1].Version != expectedVersion)
                    throw new ConcurrencyException();
            }

            var version = expectedVersion;

            try
            {
                foreach (var @event in eventsList)
                {
                    version++;
                    @event.Version = version;

                    var eventModel = new EventModel
                    {
                        TimeStamp = DateTime.UtcNow,
                        AggregateIdentifier = @event.id,
                        AggregateType = @event.Type,
                        EventOperator = @event.EventOperator,
                        Version = version,
                        EventType = @event.GetType().Name,
                        EventData = @event
                    };

                    await _eventStoreRepository.SaveAsync(eventModel);

                    if (isBatchLastLoop == null || (bool)isBatchLastLoop && @event == eventsList.Last())
                    {
                        var handlerMethod = GetHandlerMethod(@event.GetType());
                        var task = (Task<TResult>)handlerMethod.Invoke(_eventHandler, [@event]);

                        if (task == null)
                            throw new InvalidOperationException("Event handler method returned null");

                        return await task;
                    }
                }
            }
            catch
            {
                throw;
            }

            return default;
        }

        public async Task<List<Guid>> GetAggregateIdsAsync()
        {
            var eventStream = await _eventStoreRepository.FindAllAsync();
            if (eventStream == null || !eventStream.Any())
                throw new ArgumentNullException(nameof(eventStream),
                    "Could not retrieve event stream from the event store!");

            return eventStream
                .Select(x => x.AggregateIdentifier)
                .Distinct()
                .ToList();
        }

        public async Task UpdateEventStatusAsync(EventModel @event)
        {
            // 使用 UpdateAsync 更新記錄
            await _eventStoreRepository.UpdateAsync(@event);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    (_eventStoreRepository as IDisposable)?.Dispose();
                    _methodCache.Clear();
                }
                _disposed = true;
            }
        }

        ~EventStore()
        {
            Dispose(false);
        }
    }

    // 新增自定義異常類
    public class EventStoreException : Exception
    {
        public EventStoreException(string message) : base(message) { }
        public EventStoreException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
