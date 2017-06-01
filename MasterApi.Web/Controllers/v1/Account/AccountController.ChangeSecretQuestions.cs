using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Web.Filters;
using System;

namespace MasterApi.Web.Controllers.v1.Account
{
    public partial class AccountController
    {

        [HttpGet("SecretQuestion")]
        public async Task<IActionResult> GetSecretQuestions()
        {
            var secretQuestions = await _userAccountService.GetSecretQuestionsAsync(UserInfo.UserId);
            return Ok(secretQuestions);
        }

        [HttpPost("SecretQuestion")]
        [ModelStateValidator]
        public async Task<IActionResult> AddSecretQuestion(SecretQuestionInput model)
        {
            await _userAccountService.AddPasswordResetSecretAsync(UserInfo.UserId, model.Question, model.Answer);
            return Ok();
        }

        [HttpDelete("SecretQuestion/{id}")]
        public async Task<IActionResult> RemoveSecretQuestion(Guid id)
        {
            await _userAccountService.RemovePasswordResetSecretAsync(UserInfo.UserId, id);
            return Ok();
        }
       
    }

}
