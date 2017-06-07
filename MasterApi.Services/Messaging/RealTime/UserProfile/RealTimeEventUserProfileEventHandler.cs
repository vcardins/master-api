using System;
using MasterApi.Core.Events;
using MasterApi.Services.Messaging.RealTime;

namespace MasterApi.Services.Messaging.LiveUpdate.UserProfile
{
    public class RealTimeEventUserProfile : RealTimeEventHandler<UserProfileEvent>
    {
        public RealTimeEventUserProfile(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public virtual void Process(UserProfileEvent evt, object extra = null)
        {
            // OnEventHandler(evt);

            //ObjectId = evt.User.UserId.ToString(CultureInfo.InvariantCulture);
            //ObjectType = evt.ObjectType.ToString();

            //var template = GetTemplate(evt.SourceNotification, extra);
            //if (!string.IsNullOrEmpty(template))
            //    Send(UserFrom.UserId, UserFrom.Username, UserTo.UserId, UserTo.Username, 
            //        UserTo.DisplayName, evt.SourceNotification, template);


            //template = GetTemplate(evt.TargetNotification, extra);
            //if (!string.IsNullOrEmpty(template))
            //    Send(UserTo.UserId, UserTo.Username, UserFrom.UserId, UserFrom.Username, 
            //         UserFrom.DisplayName, evt.TargetNotification, template);

        }

    }
}
