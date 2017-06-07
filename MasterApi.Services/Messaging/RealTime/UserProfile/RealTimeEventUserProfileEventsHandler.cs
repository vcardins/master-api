using System;
using MasterApi.Core.EventHandling;
using MasterApi.Core.Events;

namespace MasterApi.Services.Messaging.LiveUpdate.UserProfile
{
    public class RealTimeEventUserProfileEventsHandler : RealTimeEventUserProfile,
                IEventHandler<UserProfileUpdatedEvent>,
                IEventHandler<UserProfileAvatarUpdatedEvent>
    {
        public RealTimeEventUserProfileEventsHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void Handle(UserProfileUpdatedEvent evt)
        {
            Process(evt, new { });
        }

        public void Handle(UserProfileAvatarUpdatedEvent evt)
        {
            Process(evt, new { });
        }      
    }
}