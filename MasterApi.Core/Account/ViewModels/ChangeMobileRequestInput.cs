using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class ChangeMobileRequestInput
    {
        [Required]
        public string MobilePhoneNumber { get; set; }

        [Required]
        public string NewMobilePhone { get; set; }
    }

}
