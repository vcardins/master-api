using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class ChangePasswordInput
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string NewPasswordConfirm { get; set; }
    }

}
