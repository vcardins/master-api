using System.Collections.Generic;

namespace MasterApi.Core.Messaging.Email
{
    public class EmailSubjects
    {
        public Dictionary<string, Dictionary<string, string>> Subjects { get; set; }

        public EmailSubjects(Dictionary<string, Dictionary<string, string>> subjects = null)
        {
            Subjects = subjects ?? new Dictionary<string, Dictionary<string, string>>();
        }
    }
}
