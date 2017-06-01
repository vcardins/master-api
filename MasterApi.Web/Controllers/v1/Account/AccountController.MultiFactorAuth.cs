using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Enums;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        [HttpPost("TwoFactorAuth")]
        [ModelStateValidator]
        public async Task<IActionResult> ConfigureMultiFactorAuth(TwoFactorAuthMode mode)
        {
            await _userAccountService.ConfigureTwoFactorAuthenticationAsync(UserInfo.UserId, mode);
            return Ok();
        }
       
    }

}
