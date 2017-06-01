using MasterApi.Core.Config;
using MasterApi.Core.EventHandling;
using System;
using Microsoft.Extensions.Options;

namespace MasterApi.Services.Messaging
{
    public abstract class MessagingEventHandler<TEvent> : IEventHandler
    {
        protected readonly string Module;

        protected AppSettings Settings;

        protected MessagingEventHandler(IServiceProvider serviceProvider)
        {
            var settings = (IOptions<AppSettings>)serviceProvider.GetService(typeof(IOptions<AppSettings>));
            Settings = settings.Value;
            var evtStr = typeof(TEvent).Name;
            if (!string.IsNullOrEmpty(evtStr))
            {
                Module = evtStr.Replace("Event", "");
            }
        }

        protected string GetEvent(TEvent t)
        {
            var evtStr = GetEventName(t).Replace(Module, "");
            return evtStr;
        }

        protected string GetEventName(TEvent t)
        {
            var evtStr = t.GetType().Name;
            if (!string.IsNullOrEmpty(evtStr))
            {
                evtStr = evtStr.Replace("Event", "");
            }
            return evtStr;
        }

    }

}
