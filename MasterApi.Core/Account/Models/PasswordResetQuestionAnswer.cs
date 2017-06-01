using System;

namespace MasterApi.Core.Account.Models
{
    public class PasswordResetQuestionAnswer
    {
        public Guid QuestionId { get; set; }
        public string Answer { get; set; }
    }
}
