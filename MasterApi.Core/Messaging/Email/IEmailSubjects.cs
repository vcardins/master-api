using System.Collections.Generic;

namespace MasterApi.Core.Messaging.Email
{
    public interface IEmailSubjects : IDictionary<string, Dictionary<string, string>>
    {
        
    }
}
