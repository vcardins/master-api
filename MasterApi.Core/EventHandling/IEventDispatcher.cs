namespace MasterApi.Core.EventHandling
{
    public interface IEventDispatcher
    {
        void Resolve();
        void Dispatch<TEvent>(TEvent e);
    }
  
}