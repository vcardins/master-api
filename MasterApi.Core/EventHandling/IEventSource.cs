using System.Collections.Generic;

namespace MasterApi.Core.EventHandling
{
    public interface IEventSource
    {
        IEnumerable<IEvent> GetEvents();
        void Clear();
    }
}