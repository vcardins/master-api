using System;
using System.Collections.Generic;
using MasterApi.Core.Enums;
using MasterApi.Core.EventHandling;
using MasterApi.Core.Events;
using MasterApi.Core.Models;
using MasterApi.Core.Services;
using MasterApi.Core.Config;
using Microsoft.Extensions.Options;
using MasterApi.Core.ViewModels.UserProfile;

namespace MasterApi.Services.Messaging.RealTime
{
    public abstract class RealTimeEventHandler<TEvent> : IEventHandler
    {
        public event EventHandler<NotificationEvent> Event;

        private readonly INotificationService _notificationService;

        protected Dictionary<string, object> Params;

        protected int? UserId { get; set; }

        protected string ObjectId { get; set; }

        protected string ExtraObjectId { get; set; }

        protected string ObjectType { get; set; }

        protected UserContactInfo UserFrom { get; set; }

        protected UserContactInfo UserTo { get; set; }

        protected AppSettings Settings;

        protected RealTimeEventHandler(IServiceProvider serviceProvider)
        {
            _notificationService = serviceProvider.GetService(typeof(INotificationService)) as INotificationService;
            Params = new Dictionary<string, object>();

            var settings = (IOptions<AppSettings>)serviceProvider.GetService(typeof(IOptions<AppSettings>));
            Settings = settings.Value;
        }

        protected virtual void Send(UserContactInfo userFrom, NotificationTypes? type,
            object model = null, object data = null)
        {
            if (!type.HasValue) { return; }

            var note = new Notification
            {
                UserId = userFrom.UserId,
                Type = type.GetValueOrDefault()
            };

            var result = _notificationService.Repository.Insert(note);

            Event?.Invoke(this, new NotificationEvent
            {
                Data = result,
                UserId = UserId,
                Username = userFrom.Username
            });
        }
    }
}
