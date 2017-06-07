using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Services;

namespace MasterApi.Web.Controllers.v1.Account
{
    [Route("api/{version}/[controller]")]
    public class IdentityController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController" /> class.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        public IdentityController(IUserInfo userInfo) : base(userInfo) {}

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(UserInfo);
        }
    }

}
