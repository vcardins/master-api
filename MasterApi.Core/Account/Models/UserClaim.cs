using System;
using System.Collections.Generic;
using System.Security.Claims;
using MasterApi.Core.Data;
using MasterApi.Core.Models;

namespace MasterApi.Core.Account.Models
{
    public class UserClaim : UserClaim<int>
    {
        public UserClaim(string type, string value) : base(type, value)
        {
        }

        public UserClaim()
        {
        }
    }

    public class UserClaim<TKey> : BaseObjectState, IIdentifiable<TKey> where TKey : IEquatable<TKey>
    {
        public TKey Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public virtual UserAccount User { get; set; }

        public UserClaim()
        {
        }

        public UserClaim(string type, string value)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

            Type = type;
            Value = value;
        }
    }

    public static class UserClaimCollectionExtensions
    {
        public static UserClaimCollection ToCollection(this IEnumerable<UserClaim> claims)
        {
            return new UserClaimCollection(claims);
        }
    }

    public class UserClaimCollection : HashSet<UserClaim>
    {
        public static readonly UserClaimCollection Empty = new UserClaimCollection();

        public static implicit operator UserClaimCollection(UserClaim[] claims)
        {
            return new UserClaimCollection(claims);
        }

        public static implicit operator UserClaimCollection(Claim[] claims)
        {
            return new UserClaimCollection(claims);
        }

        public UserClaimCollection()
        {
        }

        public UserClaimCollection(IEnumerable<UserClaim> claims)
        {
            if (claims == null) return;
            foreach (var claim in claims)
            {
                Add(claim);
            }
        }
        public UserClaimCollection(IEnumerable<Claim> claims)
        {
            if (claims == null) return;
            foreach (var claim in claims)
            {
                Add(claim.Type, claim.Value);
            }
        }

        public void Add(string type, string value)
        {
            Add(new UserClaim(type, value));
        }
    }
}
