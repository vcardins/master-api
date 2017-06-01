using System;
using MasterApi.Core.Account.Models;

namespace MasterApi.Core.Models
{
    public class ExceptionLog : BaseObjectState
    {
        public int Id { get; set; }
        public int HResult { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string RequestUri { get; set; }
        public string Method { get; set; }
        public string StackTrace { get; set; }
        public int? UserId { get; set; }
        public DateTimeOffset Created { get; set; }
        public virtual UserAccount User { get; set; }
    }
}
