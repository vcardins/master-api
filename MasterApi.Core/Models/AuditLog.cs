using System;
using MasterApi.Core.Account.Models;

namespace MasterApi.Core.Models
{
    public class AuditLog : BaseObjectState
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Event { get; set; }
        public DateTimeOffset DataTime { get; set; }

        public int? UserId { get; set; }
        public virtual UserAccount User { get; set; }
    }
}
