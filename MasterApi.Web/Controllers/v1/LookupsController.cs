using MasterApi.Core.Account.Services;
using MasterApi.Core.Services;
using MasterApi.Core.ViewModels;
using MasterApi.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace MasterApi.Api.Endpoints.V1
{
    /// <summary>
    /// 
    /// </summary>
    [AllowAnonymous]
    [Route("api/{version}/[controller]")]
    public class LookupController : BaseController
    {
        private readonly ILookupService _lookupService;
        /// <summary>
        /// Initializes a new instance of the <see cref="LookupController" /> class.
        /// </summary>
        /// <param name="userinfo">The userinfo.</param>
        /// <param name="lookupService">The lookup service.</param>
        public LookupController(IUserInfo userInfo, ILookupService lookupService)
            : base(userInfo)
        {
            _lookupService = lookupService;
        }

        //// GET: api/lookups/cities
        ///// <summary>
        ///// Gets visa categories.
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("cities/{country}")]
        //[ProducesResponseType(typeof(LookupOutput), (int)HttpStatusCode.OK)]
        //[ResponseType(typeof(IEnumerable<EnabledCityOutput>))]
        //public IEnumerable<EnabledCityOutput> GetEnabledCities(string country)
        //{
        //    var iso2 = User.Identity.IsAuthenticated ? User.GetClaimValue(ClaimTypes.Country) : country;
        //    return _lookupService.GetEnabledCities(iso2, User.Identity.IsAuthenticated ? User.GetUserId() : -1);
        //}

        // Lookups: aggregates the many little lookup lists in one payload
        // to reduce roundtrips when the client launches.
        // GET: api/lookups
        /// <summary>
        /// Gets the lookups.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(LookupOutput), (int)HttpStatusCode.OK)]
        public async Task<LookupOutput> GetLookups()
        {            
            return await _lookupService.GetAll();
        }
    }
}
