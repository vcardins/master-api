using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Messaging.Sms;
using MasterApi.Web.Filters;
using MasterApi.Web.Resources;

namespace MasterApi.Web.Controllers.v1
{
    /// <summary>
    /// SMS Testing controller
    /// </summary>
    /// <seealso cref="MasterApi.Web.Controllers.BaseController" />
    [AllowAnonymous]
    [Route("api/{version}/[controller]")]
    public class SmsController : BaseController
    {
        private readonly ISmsSender _smsSender;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IStringLocalizer<SmsController> _ctrlLocalizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsController" /> class.
        /// </summary>
        public SmsController(IUserInfo userinfo, ISmsSender smsSender, IStringLocalizer<SharedResource> localizer, 
            IStringLocalizer<SmsController> ctrlLocalizer) : base(userinfo)
        {
            _smsSender = smsSender;
            _localizer = localizer;
            _ctrlLocalizer = ctrlLocalizer;
        }

        /// <summary>
        /// Sends SMS.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost("")]
        [ModelStateValidator]
        public async Task<IActionResult> SendSmsAsync(SmsMessage model)
        {
            model.Message = $"{CultureInfo.CurrentCulture} {_localizer["Language"]} {_ctrlLocalizer["Language"]}";
            var response = await _smsSender.SendSmsAsync(model);
            return Ok(model);
        }
    }
}
