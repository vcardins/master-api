using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MasterApi.Core.EventHandling
{
    public class DefaultDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, Collection<Delegate>> _handlers;

        public DefaultDispatcher()
        {
            _handlers = new Dictionary<Type, Collection<Delegate>>();
        }

        public void Register<TEvent>(Action<TEvent> handler)
        {
            Collection<Delegate> eventHandlers;
            if (!_handlers.TryGetValue(typeof(TEvent), out eventHandlers))
            {
                eventHandlers = new Collection<Delegate>();
                _handlers.Add(typeof(TEvent), eventHandlers);
            }

            eventHandlers.Add(handler);
        }

        public void Resolve()
        {
            throw new NotImplementedException();
        }

        public void Dispatch<TEvent>(TEvent e)
        {
            Collection<Delegate> eventHandlers;
            if (!_handlers.TryGetValue(typeof(TEvent), out eventHandlers)) return;
            foreach (var handler in eventHandlers.Cast<Action<TEvent>>())
            {
                try
                {
                    handler(e);
                }
                catch
                {
                    // log
                }
            }
        }
    }
   
}