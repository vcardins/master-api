using MasterApi.Core.Enums;
using MasterApi.Core.Extensions;

namespace MasterApi.Core.Events
{
    public class DomainEvent
    {
        public string Module { get; set; }

        public string Action { get; set; }

        public int? UserId { get; set; }

        public object Data { get; set; }


        public DomainEvent(ModelType domain, ModelAction action)
            : this(domain, action, null)
        {
        }

        public DomainEvent(string domain, string action)
            : this(domain, action, null)
        {
        }

        public DomainEvent(string domain, string action, object data)
        {
            Module = domain;
            Action = action;
            Data = data;
        }

        public DomainEvent(ModelType domain, ModelAction action, object data)
        {
            Module = domain.GetName();
            Action = action.GetName();
            Data = data;
        }
    }
}