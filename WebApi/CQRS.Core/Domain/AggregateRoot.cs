using CQRS.Core.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CQRS.Core.Domain
{
    public abstract class AggregateRoot
    {
        protected Guid _id;
        private readonly List<BaseEvent> _changes = new();
        private IHttpContextAccessor? _httpContextAccessor;

        public Guid Id => _id;
        public int Version { get; set; } = -1;

        protected AggregateRoot()
        {
        }

        protected AggregateRoot(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<BaseEvent> GetUncommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        protected void ApplyChange(BaseEvent @event, bool isNew)
        {
            var method = GetType().GetMethod("Apply", [@event.GetType()]);
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method), $"The Apply method was not found in the aggregate for {@event.GetType().Name}!");
            }
            method.Invoke(this, [@event]);
            if (isNew)
            {
                _changes.Add(@event);
            }
        }

        protected void RaiseEvent(BaseEvent @event)
        {
            ApplyChange(@event, true);
        }

        public void ReplayEvents(IEnumerable<BaseEvent> events)
        {
            foreach (var @event in events)
            {
                ApplyChange(@event, false);
            }
        }

        protected string GetCurrentOperatorId()
        {
            if (_httpContextAccessor == null)
            {
                throw new InvalidOperationException("HttpContextAccessor is not initialized. Please ensure it is properly injected.");
            }
            return _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name)?.Value
                ?? throw new InvalidOperationException("Unable to get current user from token");
        }
    }
}
