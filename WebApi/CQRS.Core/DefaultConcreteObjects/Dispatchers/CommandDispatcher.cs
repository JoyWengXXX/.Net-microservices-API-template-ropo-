using CQRS.Core.Commands;
using CQRS.Core.Infrastructure;

namespace CQRS.Core.DefaultConcreteObjects.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<Type, Func<BaseCommand, Task<TResult>>> _handlers = new();
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void RegisterHandler<T>(Func<T, Task<TResult>> handler) where T : BaseCommand
        {
            if (_handlers.ContainsKey(typeof(T)))
            {
                throw new IndexOutOfRangeException("You cannot register the same command handler twice!");
            }

            _handlers.Add(typeof(T), x => handler((T)x));
        }

        public async Task<TResult> SendAsync(BaseCommand command)
        {
            var handlerType = typeof(Func<,>).MakeGenericType(command.GetType(), typeof(Task<TResult>));
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), $"No command handler was registered for {command.GetType().Name}!");
            }

            return await (Task<TResult>)handlerType.GetMethod("Invoke").Invoke(handler, new object[] { command });
        }
    }
}

