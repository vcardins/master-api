using System;

namespace MasterApi.Core.Messaging.PushNotification
{
    public class ParseResponse
    {
        public string ObjectId { get; set; }

        public string Code { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public string DeviceType { get; set; }

        public string DeviceToken { get; set; }

        public string Error { get; set; }

    }
}
