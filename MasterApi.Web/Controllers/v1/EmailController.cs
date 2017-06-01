using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using RazorLight;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Core.Config;
using MasterApi.Core.Messaging.Email;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1
{
    [Route("api/{version}/[controller]")]
    [AllowAnonymous]
    public class EmailController : BaseController
    {
        private readonly IEmailSender _emailSender;
        private readonly AppSettings _mySettings;
        /// <summary>
        /// The _razor engine
        /// </summary>
        private readonly IRazorLightEngine _razorEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailController" /> class.
        /// </summary>
        public EmailController(IUserInfo userinfo, IEmailSender emailSender, IOptions<AppSettings> settings, 
                IRazorLightEngine razorEngine) : base(userinfo)
        {
            _emailSender = emailSender;
            _razorEngine = razorEngine;
            _mySettings = settings.Value;
        }

        [HttpPost("")]
        [ModelStateValidator]
        public async Task<IActionResult> SendEmail([FromBody] string subject, [FromBody] string to)
        {
            var obj = new AccountCreated
            {
                FirstName = "Master API",
                Username = "masterapi",
                Email = to,
                AppInfo = _mySettings.Information
            };

            var bodyText = _razorEngine.Parse("UserAccount\\AccountCreated.cshtml", obj);

            var message = new EmailMessage
            {
                AsHtml = true,
                Subject = subject,
                Body = bodyText,
                Recipients = new List<string> { to }
            };
            await _emailSender.SendAsync(message);

            return Ok(new { Message = $"`Email Sent to { to }`"});
        }
    }
}
