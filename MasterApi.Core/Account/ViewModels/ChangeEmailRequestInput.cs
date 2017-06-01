using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class ChangeEmailRequestInput
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }

}
