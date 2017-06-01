using MasterApi.Core.Data;
using MasterApi.Core.Models;
using System;

namespace MasterApi.Core.Account.Models
{
    public class PasswordResetSecret : PasswordResetSecret<int>
    {
    }

    public class PasswordResetSecret<TKey> : BaseObjectState, IIdentifiable<TKey> where TKey : IEquatable<TKey>
    {
        public TKey Id { get; set; }
        
        public virtual Guid Guid { get; set; }
        public virtual string Question { get; set; }
        public virtual string Answer { get; set; }

        public int UserId { get; set; }
        public virtual UserAccount User { get; set; }
    }
    
}
