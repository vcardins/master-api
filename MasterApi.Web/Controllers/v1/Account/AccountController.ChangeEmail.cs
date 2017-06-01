using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {

        [HttpPost("ChangeEmail")]
        [ModelStateValidator]
        public async Task<IActionResult> ChangeEmailRequest(ChangeEmailRequestInput model)
        {
            await _userAccountService.ChangeEmailRequestAsync(UserInfo.UserId, model.NewEmail);
            if (_userAccountService.Settings.RequireAccountVerification)
            {
                return Ok(new { Message = "Change Request Success", Data = model.NewEmail });
            }
            else
            {
                return Ok(new { Message = "Email Change Success" });
            }
        }
        
    }

}
