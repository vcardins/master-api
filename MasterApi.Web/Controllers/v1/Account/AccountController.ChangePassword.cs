using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        [HttpPost("ChangePassword")]
        [ModelStateValidator]
        public async Task<IActionResult> ChangePassword(ChangePasswordInput model)
        {
            await _userAccountService.ChangePasswordAsync(UserInfo.UserId, model.OldPassword, model.NewPassword);
            return Ok();
        }
       
    }

}
