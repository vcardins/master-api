using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Core.Filters;

namespace MasterApi.Web.Identity
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MasterApi.Core.Account.Services.IUserInfo" />
    public class UserIdentityInfo : IUserInfo
    {
        private readonly ClaimsIdentity _claimsIdentity;
        private readonly ClaimsPrincipal _claimsPrincipal;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserIdentityInfo"/> class.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <exception cref="System.Exception">Invalid Identity</exception>
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

        /// <summary>
        /// Gets a value indicating whether this instance is authenticated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated => _claimsIdentity.IsAuthenticated;

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public IEnumerable<string> Roles
        {
            get { return Claims.Where(x => x.Type == ClaimTypes.Role).Select(z => z.Value).ToList(); }
        }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
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

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid => Guid.Parse(GetClaim("Guid"));

        /// <summary>
        /// Gets the authentication instant.
        /// </summary>
        /// <value>
        /// The authentication instant.
        /// </value>
        public DateTime? AuthenticationInstant
        {
            get
            {
                var dt = GetClaim(ClaimTypes.AuthenticationInstant);
                return DateTime.Parse(dt);
            }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string Username => GetClaim(ClaimTypes.Name);

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email => GetClaim(ClaimTypes.Email);

        /// <summary>
        /// Gets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IEnumerable<UserClaimOutput> Claims
        {
            get
            {
                return _claimsIdentity.Claims.Select(c => new UserClaimOutput(c.Type, c.Value)).ToList();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is admin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is admin; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdmin => _claimsPrincipal.IsInRole("Admin");

        /// <summary>
        /// Determines whether the specified type has claim.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type has claim; otherwise, <c>false</c>.
        /// </returns>
        public bool HasClaim(string type)
        {
            return _claimsIdentity.HasClaim(x => x.Type == type);
        }

        /// <summary>
        /// Determines whether [has claim value] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [has claim value] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasClaimValue(string type, string value)
        {
            return _claimsIdentity.HasClaim(x => x.Type == type && x.Value == value);
        }

        /// <summary>
        /// Determines whether [has claim value] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        ///   <c>true</c> if [has claim value] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasClaimValue(string type, List<string> values)
        {
            return _claimsIdentity.HasClaim(x => x.Type == type && values.Contains(x.Value));
        }

        /// <summary>
        /// Validates the claim.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="resources">The resources.</param>
        /// <exception cref="ForbiddenException"></exception>
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