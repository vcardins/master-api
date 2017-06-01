using System;
using System.Collections.Generic;
using MasterApi.Core.Account.ViewModels;

namespace MasterApi.Core.Account.Services
{
    public interface IUserInfo
    {
        IEnumerable<string> Roles { get; }
        int UserId { get; }
        string Username { get; }
        string Email { get; }
        bool IsAuthenticated { get; }
        Guid Guid { get; }
        DateTime? AuthenticationInstant { get; }
        bool IsAdmin { get; }
        IEnumerable<UserClaimOutput> Claims { get; }
        bool HasClaim(string type);
        bool HasClaimValue(string type, string value);
        void ValidateClaim(string type, string[] resources);
    }
}
