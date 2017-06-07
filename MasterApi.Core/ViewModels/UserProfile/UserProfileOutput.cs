using System;

namespace MasterApi.Core.ViewModels.UserProfile
{
    public class UserProfileOutput : UserProfileInput
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastLogin { get; set; }
    }
}

