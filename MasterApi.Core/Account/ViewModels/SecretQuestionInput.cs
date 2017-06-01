using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Account.ViewModels
{
    public class SecretQuestionInput
    {
        [Required]
        public string Question { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Answer { get; set; }
    }
}
