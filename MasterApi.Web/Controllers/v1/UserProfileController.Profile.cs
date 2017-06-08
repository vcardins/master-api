using MasterApi.Core.Account.Enums;
using MasterApi.Core.Extensions;
using MasterApi.Core.ViewModels.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MasterApi.Web.Controllers.v1
{
 
    public partial class ProfileController : BaseController
    {
        /// <summary>
        /// Gets the user profile.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IActionResult> GetProfileAsync()
        {
            var profile = await _userProfileService.GetAsync(UserInfo.UserId);
            return profile == null ?
                   NotFound(UserAccountMessages.UserNotFound.GetDescription()) :
                   Ok(profile);
        }

        /// <summary>
        /// Updates the user profile.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPut("")]
        public async Task<IActionResult> UpdateProfileAsync(UserProfileInput model)
        {
            var profile = await _userProfileService.UpdateProfileAsync(UserInfo.UserId, model);
            return profile == null ?
                   NotFound(UserAccountMessages.UserNotFound.GetDescription()) :
                   Ok(profile);
        }
    }

}
