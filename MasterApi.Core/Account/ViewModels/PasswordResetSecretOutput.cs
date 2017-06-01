using System;

namespace MasterApi.Core.Account.ViewModels
{
    public class PasswordResetSecretOutput
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Question { get; set; }
        //public string Answer { get; set; }
        //public int UserId { get; set; }
    }
    
}
