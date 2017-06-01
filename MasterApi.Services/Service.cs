using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.EventHandling;
using MasterApi.Core.Services;

namespace MasterApi.Services
{
    public class Service<TEntity> : IService<TEntity> where TEntity : class, IObjectState
    {
        #region Private Fields
        protected string Module { get; }
        protected ResourceManager ResourceManager;
        protected IUnitOfWorkAsync UnitOfWork { get; }
        public IRepositoryAsync<TEntity> Repository { get; }
        protected EventsHandler EventsHandler { get; set; }
        #endregion Private Fields

        #region Constructor

        public Service(IUnitOfWorkAsync unitOfWork)
        {
            UnitOfWork = unitOfWork;
            Repository = UnitOfWork.RepositoryAsync<TEntity>();
            EventsHandler = new EventsHandler();
            Module = typeof(TEntity).Name;
        }
        #endregion Constructor

        protected readonly List<IEvent> Events = new List<IEvent>();
        protected readonly CommandBus CommandBus = new CommandBus();

        IEnumerable<IEvent> IEventSource.GetEvents()
        {
            return Events;
        }

        void IEventSource.Clear()
        {
            Events.Clear();
        }

        public void ClearEvents()
        {
            Events.Clear();
        }

        public void AddEventHandler(params IEventHandler[] handlers)
        {
            EventsHandler.AddEventHandler(handlers);
        }

        public void AddCommandHandler(ICommandHandler handler)
        {
            CommandBus.Add(handler);
        }
        
        public void ExecuteCommand(ICommand cmd)
        {
            CommandBus.Execute(cmd);
            //Configuration.CommandBus.Execute(cmd);
        }

        public string GetValidationMessage(string id)
        {
            var cmd = new GetValidationMessage { Id = id };
            ExecuteCommand(cmd);
            if (cmd.Message != null) return cmd.Message;

            var result = string.Empty; //ResourceManager.GetString(id, Resources.Culture);
            if (result == null) throw new Exception("Missing validation message for ID : " + id);
            return result;
        }        

        public void AddEvent(IEvent evt)
        {
            //evt is IAllowMultiple || 
            if (Events.All(x => x.GetType() != evt.GetType()))
            {
                Events.Add(evt);
            }
        }

        public void RaiseEvent(IEvent evt)
        {
            //if (evt is IAllowMultiple)
            EventsHandler.EventBus.RaiseEvent(evt);
        }

        public void FireEvents()
        {
            foreach (var ev in Events)
            {
                var ev1 = ev;
                EventsHandler.EventBus.RaiseEvent(ev1);
            }
        }
    }

    public class GetValidationMessage : ICommand
    {
        public string Id { get; set; }
        public string Message { get; set; }
    }
}