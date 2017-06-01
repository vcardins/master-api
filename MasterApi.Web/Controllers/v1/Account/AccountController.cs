using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RazorLight;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Config;
using MasterApi.Services.Account.Messaging;
using Microsoft.AspNetCore.DataProtection;

namespace MasterApi.Web.Controllers.v1.Account
{
    [Route("api/{version}/[controller]")]
    public partial class AccountController : BaseController
    {
        private readonly IUserAccountService _userAccountService;
        private readonly AppSettings _appSettings;
        private readonly IDataProtector _protector;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController" /> class.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <param name="userAccountService">The user account service.</param>
        /// <param name="razorEngine">The razor engine.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="provider"></param>
        public AccountController(IUserInfo userInfo, IUserAccountService userAccountService, 
            IRazorLightEngine razorEngine, IOptions<AppSettings> settings, 
            IServiceProvider serviceProvider,
            IDataProtectionProvider provider) 
            : base(userInfo)
        {
            _appSettings = settings.Value;
            _userAccountService = userAccountService;

            var watcher = new RealTimeUserAccountEventsHandler(serviceProvider);
            watcher.Event += HandleChange;
            _userAccountService.AddEventHandler(watcher);

            _protector = provider.CreateProtector(GetType().FullName);
        }
       
    }

}
