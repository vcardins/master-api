using System;
using System.Collections.Generic;
using System.Security.Claims;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.ViewModels;

namespace MasterApi.Core.Account.Models
{
    public class UserAccountAuthentication
    {
        public Guid Guid { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserProfileOutput Profile { get; set; }
        public List<Claim> Claims { get; set; }
        public UserAccountAuthStatus? Status { get; set; }
        public UserAccountAuthentication()
        {
            Claims = new List<Claim>();
            Profile = new UserProfileOutput();
        }
    }
}
