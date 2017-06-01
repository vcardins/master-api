using System;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Enums;

namespace MasterApi.Core.Models
{
    public class Notification : BaseObjectState
    {
        public int Id { get; set; }
        public NotificationTypes Type { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Created { get; set; }
        public int? UserId { get; set; }
        public virtual UserAccount User { get; set; }
    }
}
