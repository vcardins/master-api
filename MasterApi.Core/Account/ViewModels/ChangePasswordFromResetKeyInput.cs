using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class ChangePasswordFromResetKeyInput
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Password confirmation must match password.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string Key { get; set; }
    }

}
