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
        /// Hanfles email changing request.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("ChangeEmail")]
        [ModelStateValidator]
        [ProducesResponseType(typeof(ActionResponse), 200)]
        public async Task<IActionResult> ChangeEmailRequestAsync(ChangeEmailRequestInput model)
        {
            await _userAccountService.ChangeEmailRequestAsync(UserInfo.UserId, model.NewEmail);
            if (_userAccountService.Settings.RequireAccountVerification)
            {
                return Ok(new ActionResponse { Message = "Change Request Success", Data = model.NewEmail });
            }
            else
            {
                return Ok(new ActionResponse { Message = "Email Change Success" });
            }
        }
        
    }

}
