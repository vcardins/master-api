using MasterApi.Core.Models;
using MasterApi.Core.Enums;

namespace MasterApi.Core.Events
{
    public abstract class ExceptionLogEvent : BaseEvent
    {
        public int ExceptionLogId { get; set; }
        public ExceptionLog Exception { get; set; }

        protected ExceptionLogEvent()
        {
            ModelType = ModelType.ExceptionLog;
        }
    }

    public class ExceptionLogCreatedEvent : ExceptionLogEvent
    {
        public ExceptionLogCreatedEvent(ExceptionLog err)
        {
            Exception = err;
        }
    }

}