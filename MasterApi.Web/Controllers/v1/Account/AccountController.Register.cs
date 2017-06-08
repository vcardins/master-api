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
        /// <summary>
        /// Registers user account.
        /// </summary>
        /// <param name="model">The user registration model.</param>
        /// <returns></returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ModelStateValidator]
        public async Task<IActionResult> RegisterAsync(RegisterInput model)
        {
            await _userAccountService.CreateAccountAsync(model);
            return Ok(new { Message = "Registered" });
        }

        /// <summary>
        /// Verifies user account.
        /// </summary>
        /// <param name="key">The user account verification key.</param>
        /// <returns></returns>
        [HttpGet("verify/{key}", Name = "VerifyAccount")]
        [AllowAnonymous]
        public async Task<HttpResponse> VerifyAsync(string key)
        {
            var error = await _userAccountService.VerifyEmailFromKeyAsync(key);
            return RedirectTo("VerifyAccount", error);
        }

        /// <summary>
        /// Requests the account verification.
        /// </summary>
        /// <param name="email">The user email to account be verified against.</param>
        /// <returns></returns>
        [HttpPost("verify", Name = "RequestAccountVerification")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponse> RequestAccountVerificationAsync(string email)
        {
            var error = await _userAccountService.RequestVerificationAsync(email);
            return RedirectTo("RequestAccountVerification", error);
        }

        /// <summary>
        /// Cancels the account verification.
        /// </summary>
        /// <param name="key">The user verification key.</param>
        /// <returns></returns>
        [HttpGet("verify/cancel/{key}", Name = "CancelAccountVerification")]
        [AllowAnonymous]
        public async Task<HttpResponse> CancelAccountVerificationAsync(string key)
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
