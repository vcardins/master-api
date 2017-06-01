using System;

namespace MasterApi.Core.EventHandling
{
    public class DelegateEventHandler<T> : IEventHandler<T>
        where T : IEvent
    {
        private readonly Action<T> _action;
        public DelegateEventHandler(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        public void Handle(T evt)
        {
            _action(evt);
        }
    }
}
