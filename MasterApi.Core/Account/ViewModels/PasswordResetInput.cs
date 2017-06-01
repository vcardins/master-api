using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class PasswordResetInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
