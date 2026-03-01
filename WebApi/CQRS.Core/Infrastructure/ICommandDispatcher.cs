using CQRS.Core.Commands;

namespace CQRS.Core.Infrastructure
{
    public interface ICommandDispatcher
    {
        void RegisterHandler<T>(Func<T, Task<TResult>> handler) where T : BaseCommand;
        Task<TResult> SendAsync(BaseCommand command);
    }

    public class TResult()
    {
        public bool isSuccess { get; set; }
        public object executionData { get; set; }
    }
}

