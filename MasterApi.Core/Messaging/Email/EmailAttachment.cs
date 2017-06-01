using System.IO;

namespace MasterApi.Core.Messaging.Email
{
   
    public class EmailAttachment
    {
        public string Name { get; set; }

        public MemoryStream File { get; set; }

    }
}
