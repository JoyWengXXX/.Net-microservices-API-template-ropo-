using CQRS.Core.Domain;
using CQRS.Core.Infrastructure;

namespace CQRS.Core.Handlers
{
    public interface IEventSourcingHandler<T>
    {
        Task<TResult> SaveAsync(AggregateRoot aggregate, bool isEventScourcingEnable);
        Task<TResult> SaveAsync(AggregateRoot aggregate, bool isEventScourcingEnable, bool isBatchLastLoop)
        {
            return SaveAsync(aggregate, isEventScourcingEnable);
        }
        Task<T> GetByIdAsync(Guid aggregateId, bool isEventScourcingEnable);
        Task RepublishEventsAsync(bool isEventScourcingEnable);
    }

    public interface IBatchedEventSourcingHandler<T> : IEventSourcingHandler<T>
    {
        // 滦籠膀セㄢ把计よ猭矗ㄑ箇砞龟秸ノ把计セ
        new Task<TResult> SaveAsync(AggregateRoot aggregate, bool isEventScourcingEnable)
        {
            return SaveAsync(aggregate, isEventScourcingEnable, false);
        }
        
        // 璶―龟把计セ
        new Task<TResult> SaveAsync(AggregateRoot aggregate, bool isEventScourcingEnable, bool isBatchLastLoop);
    }
}

