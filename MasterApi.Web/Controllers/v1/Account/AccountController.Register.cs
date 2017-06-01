using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        [HttpPost("register")]
        [AllowAnonymous]
        [ModelStateValidator]
        public async Task<IActionResult> Register(RegisterInput model)
        {
            await _userAccountService.CreateAccountAsync(model);
            return Ok(new { Message = "Registered" });
        }

        [HttpGet("verify/{key}", Name = "VerifyAccount")]
        [AllowAnonymous]
        public async Task<HttpResponse> Verify(string key)
        {
            var error = await _userAccountService.VerifyEmailFromKeyAsync(key);
            return RedirectTo("VerifyAccount", error);
        }

        [HttpPost("verify", Name = "RequestAccountVerification")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponse> RequestAccountVerification(string email)
        {
            var error = await _userAccountService.RequestVerificationAsync(email);
            return RedirectTo("RequestAccountVerification", error);
        }

        [HttpGet("verify/cancel/{key}", Name = "CancelAccountVerification")]
        [AllowAnonymous]
        public async Task<HttpResponse> CancelAccountVerification(string key)
        {
            var error = await _userAccountService.CancelVerificationAsync(key);
            return RedirectTo("CancelAccountVerification", error);
        }

        private HttpResponse RedirectTo(string action, string error)
        {
            var redirectTo = !string.IsNullOrEmpty(error) ?
                             new Uri(string.Format("{0}{1}?e={2}", _appSettings.Urls.Web, _appSettings.Urls.ErrorPage, error)) :
                             new Uri(string.Format("{0}{1}", _appSettings.Urls.Web, _appSettings.Urls.LoginPage));

            Response.Headers.Add("event", string.IsNullOrEmpty(error) ? error : action);
            Response.Redirect(redirectTo.ToString());
            return Response;
        }
    }

}
