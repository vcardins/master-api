using System;
using MasterApi.Core.Data;
using MasterApi.Core.Models;

namespace MasterApi.Core.Account.Models
{
    public class TwoFactorAuthToken : TwoFactorAuthToken<int>
    {
    }

    public class TwoFactorAuthToken<TKey> : BaseObjectState, IIdentifiable<TKey> where TKey : IEquatable<TKey>
    {
        public TKey Id { get; set; }
        public int UserId { get; set; }
        public virtual string Token { get; set; }
        public virtual DateTime Issued { get; set; }
        public virtual UserAccount User { get; set; }
    }

}
