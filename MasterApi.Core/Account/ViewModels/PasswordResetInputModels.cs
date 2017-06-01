using System;
using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class PasswordResetWithSecretInputModel
    {       
        public PasswordResetSecretViewModel[] Questions { get; set; }
        [Required]
        public string ProtectedAccountID { get; set; }
    }

    public class PasswordResetSecretViewModel : PasswordResetSecretInputModel
    {
        public string Question { get; set; }
    }

    public class PasswordResetSecretInputModel
    {
        public Guid QuestionId { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Answer { get; set; }
    }
}
