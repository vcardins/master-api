namespace MasterApi.Core.Account.ViewModels
{

    public class UserClaimOutput
    {
        public UserClaimOutput(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }
        public string Value { get; set; }
    }

}
