using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        [HttpPost("ChangeUsername")]
        [ModelStateValidator]
        public async Task<IActionResult> ChangeUsernameRequest(ChangeUsernameRequestInput model)
        {
            await _userAccountService.ChangeUsernameAsync(UserInfo.UserId, model.NewUsername);

            if (_userAccountService.Settings.RequireAccountVerification)
            {
                return Ok(new { Message = "Change Request Success", Data = model.NewUsername });
            }
            else
            {
                return Ok(new { Message = "Username Change Success"});
            }
        }
    }

}
