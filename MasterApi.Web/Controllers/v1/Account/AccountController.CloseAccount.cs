using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Http;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        /// <summary>
        /// Closes the user account.
        /// </summary>
        /// <param name="guid">The user account unique identifier.</param>
        /// <returns></returns>
        [HttpPost("close/{guid}")]
        public async Task<HttpResponse> CloseAccountAsync(Guid guid)
        {
            await _userAccountService.DeleteAccountAsync(guid);
            Response.Redirect(_appSettings.Urls.Web);
            return Response;
        }
       
    }

}
