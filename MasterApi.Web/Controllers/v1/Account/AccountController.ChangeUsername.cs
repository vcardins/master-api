using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;
using MasterApi.Web.ViewModels;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        /// <summary>
        /// Changes the username request.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("ChangeUsername")]
        [ModelStateValidator]
        public async Task<IActionResult> ChangeUsernameRequestAsync(ChangeUsernameRequestInput model)
        {
            await _userAccountService.ChangeUsernameAsync(UserInfo.UserId, model.NewUsername);

            if (_userAccountService.Settings.RequireAccountVerification)
            {
                return Ok(new ActionResponse { Message = "Change Request Success", Data = model.NewUsername });
            }
            else
            {
                return Ok(new ActionResponse { Message = "Username Change Success"});
            }
        }
    }

}
