using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        /// <summary>
        /// Changes the password request asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("ChangePassword")]
        [ModelStateValidator]
        public async Task<IActionResult> ChangePasswordRequestAsync(ChangePasswordInput model)
        {
            await _userAccountService.ChangePasswordAsync(UserInfo.UserId, model.OldPassword, model.NewPassword);
            return Ok();
        }
       
    }

}
