using System.Collections.Generic;
using MasterApi.Core.Infrastructure.Storage;

namespace MasterApi.Core.Config
{
    public class AppSettings
    {
        public AppInformation Information { get; set; }
        public string SecretKey { get; set; }
        public bool InMemoryProvider { get; set; }
        public AppBaseUrls Urls { get; set; }
        public AuthSettings Auth { get; set; }
        public TwilioSettings Twilio { get; set; }
        public EmailSettings Email { get; set; }
        public List<BlobStorageProviderSettings> BlobStorageProviders { get; set; }
        public BlobStorageProvider DefaultStorageProvider { get; set; }

        public AppSettings()
        {
            Urls = new AppBaseUrls();
        }
    }
}
