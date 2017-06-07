using System;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using MasterApi.Services.Messaging.LiveUpdate.UserProfile;
using MasterApi.Core.Infrastructure.Storage;
using MasterApi.Core.Config;
using Microsoft.Extensions.Options;
using System.Linq;

namespace MasterApi.Web.Controllers.v1
{
    [Route("api/{version}/[controller]")]
    public partial class ProfileController : BaseController
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IMemoryCache _cache;
        private static MemoryCacheEntryOptions _cacheOptions;
        private readonly BlobStorageProviderSettings _blobSettings;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileController" /> class.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <param name="userProfileService">The country service.</param>
        public ProfileController(IUserInfo userInfo, IOptions<AppSettings> settings, IServiceProvider serviceProvider, IUserProfileService userProfileService, IMemoryCache cache) : base(userInfo)
        {
            _userProfileService = userProfileService;
            _cache = cache;
            _cacheOptions = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove,
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };

            _blobSettings =
               settings.Value.BlobStorageProviders.FirstOrDefault(x => x.Provider == settings.Value.DefaultStorageProvider);
            _userProfileService.AddEventHandler(new RealTimeEventUserProfileEventsHandler(serviceProvider));
        }
        
    }

}
