using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Http;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        [HttpPost("close/{guid}")]
        public async Task<HttpResponse> CloseAccount(Guid guid)
        {
            await _userAccountService.DeleteAccountAsync(guid);
            Response.Redirect(_appSettings.Urls.Web);
            return Response;
        }
       
    }

}
