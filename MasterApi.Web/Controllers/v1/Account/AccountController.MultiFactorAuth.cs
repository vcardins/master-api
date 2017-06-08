using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Enums;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        /// <summary>
        /// Configures the multi factor authentication.
        /// </summary>
        /// <param name="mode">The TwoFactorAuthMode mode.</param>
        /// <returns></returns>
        [HttpPost("TwoFactorAuth")]
        [ModelStateValidator]
        public async Task<IActionResult> ConfigureMultiFactorAuthAsync(TwoFactorAuthMode mode)
        {
            await _userAccountService.ConfigureTwoFactorAuthenticationAsync(UserInfo.UserId, mode);
            return Ok();
        }
       
    }

}
