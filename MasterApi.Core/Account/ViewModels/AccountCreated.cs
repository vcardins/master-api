using MasterApi.Core.Config;

namespace MasterApi.Core.Account.ViewModels
{
    public class AccountCreated
    {
        public string FirstName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public AppInformation AppInfo { get; set; }
    }

}
