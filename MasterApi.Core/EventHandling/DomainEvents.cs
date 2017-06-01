using System;

namespace MasterApi.Core.EventHandling
{
    public static class DomainEvents
    {
        public static IEventDispatcher Dispatcher { get; set; }

        static DomainEvents()
        {
            Dispatcher = new EmptyDispatcher();
        }

        public static void Raise<TEvent>(TEvent e)
        {
            if (e != null)
            {
                Dispatcher.Dispatch(e);
            }
        }

        private class EmptyDispatcher : IEventDispatcher
        {
            public void Resolve()
            {
                throw new NotImplementedException();
            }

            public void Dispatch<TEvent>(TEvent e) { }
        }
    }
   
}