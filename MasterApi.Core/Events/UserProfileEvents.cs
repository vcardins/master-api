using System.Collections.Generic;
using MasterApi.Core.Enums;
using MasterApi.Core.Models;

namespace MasterApi.Core.Events
{
    public abstract class UserProfileEvent : BaseEvent
    {
        public UserProfile User { get; set; }

        public object Object { get; set; }

        public List<string> Recepients { get; set; }       

        protected UserProfileEvent()
        {
            ModelType = ModelType.UserProfile;
        }
    }

    public class UserProfileUpdatedEvent : UserProfileEvent
    {
        public UserProfileUpdatedEvent()
        {
            Action = ModelAction.Update;
        }
    }

    public class UserProfileAvatarUpdatedEvent : UserProfileEvent
    {
        public UserProfileAvatarUpdatedEvent()
        {
            ModelType = ModelType.UserAvatar;
            Action = ModelAction.Update;
        }
    }

    public class UserProfileDeletedEvent : UserProfileEvent
    {
        public UserProfileDeletedEvent()
        {
            Action = ModelAction.Delete;
        }
    }

}