
using MasterApi.Core.Account.Enums;

namespace MasterApi.Core.Account.Models
{
    public class MobileInfo
    {
        public string InstallationId { get; set; }
        public string ObjectId { get; set; }
        public MobileOS Os { get; set; }
        public string Version { get; set; }
        public string Device { get; set; }
        public MobileState State { get; set; }
        public string Token { get; set; }
    }
}
