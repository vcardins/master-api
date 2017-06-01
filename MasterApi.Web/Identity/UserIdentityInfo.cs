using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Core.Filters;

namespace MasterApi.Web.Identity
{
    public class UserIdentityInfo : IUserInfo
    {
        private readonly ClaimsIdentity _claimsIdentity;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public UserIdentityInfo(ClaimsPrincipal claimsPrincipal)
        {
            _claimsPrincipal = claimsPrincipal ?? throw new Exception("Invalid Identity");
            _claimsIdentity = _claimsPrincipal.Identity as ClaimsIdentity;
        }

        private string GetClaim(string claim)
        {            
            var value = _claimsIdentity.FindFirst(claim);
            return value?.Value;
        }

        public bool IsAuthenticated => _claimsIdentity.IsAuthenticated;

        public IEnumerable<string> Roles
        {
            get { return Claims.Where(x => x.Type == ClaimTypes.Role).Select(z => z.Value).ToList(); }
        }

        public int UserId
        {
            get
            {
                if (int.TryParse(GetClaim(ClaimTypes.NameIdentifier), out int g))
                {
                    return g;
                }
                return -1;
            }
        }

        public Guid Guid => Guid.Parse(GetClaim("Guid"));

        public DateTime? AuthenticationInstant
        {
            get
            {
                var dt = GetClaim(ClaimTypes.AuthenticationInstant);
                return DateTime.Parse(dt);
            }
        }

        public string Username => GetClaim(ClaimTypes.Name);

        public string Email => GetClaim(ClaimTypes.Email);

        public IEnumerable<UserClaimOutput> Claims
        {
            get
            {
                return _claimsIdentity.Claims.Select(c => new UserClaimOutput(c.Type, c.Value)).ToList();
            }
        }

        public bool IsAdmin => _claimsPrincipal.IsInRole("Admin");

        public bool HasClaim(string type)
        {
            return _claimsIdentity.HasClaim(x => x.Type == type);
        }

        public bool HasClaimValue(string type, string value)
        {
            return _claimsIdentity.HasClaim(x => x.Type == type && x.Value == value);
        }

        public void ValidateClaim(string type, string[] resources)
        {
            var claim = _claimsIdentity.Claims.FirstOrDefault(c => c.Type == type && resources.Contains(c.Value));
            if (claim == null)
            {
                throw new ForbiddenException();
            }
        }
    }
}