
namespace MasterApi.Core.Infrastructure.Storage
{
    public class BlobStorageProviderSettings
    {
        public BlobStorageProvider Provider { get; set; }
        public string Account { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public string BaseDeliveryUrl { get; set; }
        public string SecureDeliveryUrl { get; set; }
        public string ApiBaseUrl { get; set; }
    }
}
