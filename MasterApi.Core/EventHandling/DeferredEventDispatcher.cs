using System;
using System.Collections.Concurrent;

namespace MasterApi.Core.EventHandling
{
    public class DeferredEventDispatcher : IEventDispatcher
    {
        private readonly IEventDispatcher _inner;
        private readonly ConcurrentQueue<Action> _events = new ConcurrentQueue<Action>();

        public DeferredEventDispatcher(IEventDispatcher inner)
        {
            _inner = inner;
        }

        public void Dispatch<TEvent>(TEvent e)
        {
            _events.Enqueue(() => _inner.Dispatch(e));
        }

        public void Resolve()
        {
            Action dispatch;
            while (_events.TryDequeue(out dispatch))
            {
                dispatch();
            }
        }
    }
   
}