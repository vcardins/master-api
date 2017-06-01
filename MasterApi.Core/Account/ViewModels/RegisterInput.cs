using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class RegisterInput
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Password confirmation must match password.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string MobilePhoneNumber { get; set; }

        public string City { get; set; }

        public string ProvinceState { get; set; }

        [MaxLength(2)]
        public string Country { get; set; }        
    }

}
