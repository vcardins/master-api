using System;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.Models;

namespace MasterApi.Core.Account.Models
{
    public class UserMobileDevice : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserProfile Owner { get; set; }
        public string OS { get; set; }
        public string Token { get; set; }
        public string InstallationId { get; set; }
        public string ObjectId { get; set; }
        public string Device { get; set; }
        public string Version { get; set; }
        public bool Active { get; set; }
        public MobileState State { get; set; }
        public DateTimeOffset? StateUpdated { get; set; }
        public MobileOS MobileOs => GetMobileOs(OS);

        public static MobileOS GetMobileOs(string os)
        {
            switch (os.ToUpper())
            {
                case "IPHONE":
                case "IOS": return MobileOS.iOS;
                case "ANDROID": return MobileOS.Android;
                case "BLACKBERRY": return MobileOS.BlackBerry;
                case "WINDOWS": return MobileOS.WindowsPhone;
                default:
                    return MobileOS.Other;
            }
        }
    }
}
