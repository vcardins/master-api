using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        [HttpPost("ChangeMobile")]
        [ModelStateValidator]
        public async Task<IActionResult> ChangeMobilePhoneRequest(ChangeMobileRequestInput model)
        {
            await _userAccountService.ChangeMobilePhoneRequestAsync(UserInfo.UserId, model.NewMobilePhone);
            return Ok(new { Message = "Change Request Success" });
        }
    }

}
