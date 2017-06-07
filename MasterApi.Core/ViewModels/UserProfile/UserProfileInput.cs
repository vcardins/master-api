using MasterApi.Core.Extensions;
using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.ViewModels.UserProfile
{
    public class UserProfileInput
    {
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        private string _displayName;
        public string DisplayName
        {
            get
            {
                return !string.IsNullOrEmpty(_displayName)
                    ? _displayName
                    : FirstName.GetDisplayName(LastName);
            }
            set { _displayName = value; }
        }
        public string Avatar { get; set; }
        public string Iso2 { get; set; }
        public string ProvinceState { get; set; }
        public string City { get; set; }
    }
}
