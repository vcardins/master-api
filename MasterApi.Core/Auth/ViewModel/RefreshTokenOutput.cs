using System;

namespace MasterApi.Core.Auth.ViewModel
{
    public class RefreshTokenOutput
    {
        public string ClientId { get; set; }        
        public string Subject { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public string ProtectedTicket { get; set; }
    }
}
