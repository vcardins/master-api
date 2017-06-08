using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;
using System;
using System.Collections.Generic;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {

        /// <summary>
        /// Gets the account secret questions.
        /// </summary>
        /// <returns></returns>        
        [HttpGet("SecretQuestion")]
        [ProducesResponseType(typeof(List<PasswordResetSecretOutput>), 200)]
        public async Task<IActionResult> GetSecretQuestionsAsync()
        {
            var secretQuestions = await _userAccountService.GetSecretQuestionsAsync(UserInfo.UserId);
            return Ok(secretQuestions);
        }

        /// <summary>
        /// Adds the account secret questions.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("SecretQuestion")]
        [ProducesResponseType(200)]
        [ModelStateValidator]
        public async Task<IActionResult> AddSecretQuestionAsync(SecretQuestionInput model)
        {
            await _userAccountService.AddPasswordResetSecretAsync(UserInfo.UserId, model.Question, model.Answer);
            return Ok();
        }

        /// <summary>
        /// Removes account secret questions.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpDelete("SecretQuestion/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveSecretQuestionAsync(Guid id)
        {
            await _userAccountService.RemovePasswordResetSecretAsync(UserInfo.UserId, id);
            return Ok();
        }
       
    }

}
