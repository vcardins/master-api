using System;

namespace MasterApi.Core.ViewModels
{
    public class UserProfileOutput
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string City { get; set; }
        public string Iso2 { get; set; }
        public string ProvinceState { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}

