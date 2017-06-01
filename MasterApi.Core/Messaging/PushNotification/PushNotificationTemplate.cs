
namespace MasterApi.Core.Messaging.PushNotification
{
    public class PushNotificationTemplate : MessageTemplate
    {
        public string Sound { get; set; }

        public bool Vibrate { get; set; }

        public bool Silent { get; set; }

    }
  
}
