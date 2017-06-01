using System;
using MasterApi.Core.Models;

namespace MasterApi.Core.Auth.Models
{
    public class RefreshToken : BaseObjectState
    {
        public string Id { get; set; }        
        public string ClientId { get; set; }
        public Audience Audience { get; set; }
        public string Subject { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public string ProtectedTicket { get; set; }
    }
}
