namespace MasterApi.Core.ViewModels
{
    public class UserContactInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        private string _displayName;
        public string DisplayName
        {
            get
            {
                return !string.IsNullOrEmpty(_displayName)
                    ? _displayName
                    : string.Format("{0} {1}", FirstName,
                        string.IsNullOrEmpty(LastName) ? string.Empty : LastName.Substring(1));
            }
            set { _displayName = value;  }
        }
       
    }
}

