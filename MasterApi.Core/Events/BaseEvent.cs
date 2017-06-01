using MasterApi.Core.Enums;
using MasterApi.Core.EventHandling;

namespace MasterApi.Core.Events
{
    public abstract class BaseEvent : IEvent
    {
        public int ObjectId { get; set; }
        public ModelType ModelType { get; set; }
        public ModelAction Action { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
   
}