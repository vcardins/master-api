using System;

namespace MasterApi.Core.ViewModels
{
    public class NotificationOutput
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Created { get; set; }
        public int? UserId { get; set; }
    }
}
