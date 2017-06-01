using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.EventHandling;

namespace MasterApi.Core.Services
{
    public interface IService<TEntity> : IEventSource where TEntity : class, IObjectState
    {
        IRepositoryAsync<TEntity> Repository { get; }
        void AddEvent(IEvent evt);
        void RaiseEvent(IEvent evt);
        void FireEvents();
        void ClearEvents();
        string GetValidationMessage(string id);
        void AddEventHandler(params IEventHandler[] handlers);
    }

}