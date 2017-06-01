using System;
using MasterApi.Core.Models;

namespace MasterApi.Core.Account.Models
{
    public class ExternalLogin : BaseObjectState
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }        
        public virtual DateTimeOffset LastLogin { get; set; }
        public int UserId { get; set; }
        public virtual UserAccount User { get; set; }
    }
}
