namespace MasterApi.Core.EventHandling
{
    public interface IEventBus
    {
        void RaiseEvent(IEvent evt);
    }
}