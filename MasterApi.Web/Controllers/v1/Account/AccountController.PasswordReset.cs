using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Omu.ValueInjecter;
using MasterApi.Core.Account.Enums;
using System.Security.Claims;
using MasterApi.Core.Extensions;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;
using MasterApi.Web.Identity;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {
        /// <summary>
        /// Resets the user account password.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        //[ValidateAntiForgeryToken]
        [ModelStateValidator]
        public async Task<IActionResult> ResetPasswordRequestAsync(PasswordResetInput model)
        {
            var account = await _userAccountService.GetByEmailAsync(model.Email, x => x.PasswordResetSecretCollection);
            if (account==null)
            {
                ModelState.AddModelError("", "Invalid email");
                return BadRequest(ModelState.Errors());
            }

            if (!account.PasswordResetSecretCollection.Any())
            {
                await _userAccountService.ResetPasswordAsync(model.Email);
                return Ok();
            }

            var bytes = Encoding.UTF8.GetBytes(account.Guid.ToString());
            bytes = _protector.Protect(bytes);
            
            var vm = new PasswordResetWithSecretInputModel {
                ProtectedAccountID = Convert.ToBase64String(bytes),
                Questions = account.PasswordResetSecretCollection.Select(
                    x => new PasswordResetSecretViewModel
                    {
                        QuestionId = x.Guid,
                        Question = x.Question
                    }).ToArray()
            };

            return Ok(vm);
        }

        /// <summary>
        /// Resets the user account password with questions.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ResetPasswordWithQuestions")]
        //[ValidateAntiForgeryToken]
        [ModelStateValidator]
        public async Task<IActionResult> ResetPasswordWithQuestionsAsync(PasswordResetWithSecretInputModel model)
        {

            var answers =
                model.Questions.Select(x => new PasswordResetQuestionAnswer().InjectFrom(x)).Cast<PasswordResetQuestionAnswer>();

            var bytes = Convert.FromBase64String(model.ProtectedAccountID);
            bytes = _protector.Unprotect(bytes);
            var val = Encoding.UTF8.GetString(bytes);
            var accountId = Guid.Parse(val);

            await _userAccountService.ResetPasswordFromSecretQuestionAndAnswerAsync(accountId, answers.ToArray());
            return Ok();
        }

        /// <summary>
        /// Confirms user account reset password.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ConfirmResetPassword")]
        [ModelStateValidator]
        public async Task<IActionResult> ConfirmResetPasswordAsync(ChangePasswordFromResetKeyInput model)
        {
            var account = await _userAccountService.ChangePasswordFromResetKeyAsync(model.Key, model.Password);

            Task<ClaimsIdentity> claimsIdentity;
            UserAccountMessages failure;

            if (await _userAccountService.AuthenticateAsync(account.Username, model.Password, out claimsIdentity,
                out failure)) {
                return Ok(new {Message = "Password has been succesfully changed. You can now login"});
            }

            var error = new AuthError
            {
                Error = "invalid_grant",
                ErrorDescription = failure.GetDescription()
            };

            return BadRequest(error);
        }

    }

}
