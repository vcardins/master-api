using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class ChangeUsernameRequestInput
    {
        [Required]
        public string NewUsername { get; set; }
    }

}
