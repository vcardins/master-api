using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        /// <summary>
        /// Changes the mobile phone request.
        /// </summary>
        /// <param name="model">The ChangeMobileRequestInput model.</param>
        /// <returns></returns>
        [HttpPost("ChangeMobile")]
        [ModelStateValidator]
        public async Task<IActionResult> ChangeMobilePhoneRequestAsync(ChangeMobileRequestInput model)
        {
            await _userAccountService.ChangeMobilePhoneRequestAsync(UserInfo.UserId, model.NewMobilePhone);
            return Ok(new { Message = "Change Request Success" });
        }
    }

}
