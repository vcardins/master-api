
namespace MasterApi.Core.Events
{
    public class NotificationEvent : BaseEvent
    {
        public string Username { get; set; }

        public int? UserId { get; set; }

        public string Event { get; set; }
    }
}