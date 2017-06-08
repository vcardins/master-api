using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Attributes;
using MasterApi.Core.Auth.Models;
using MasterApi.Core.Auth.ViewModel;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Enums;
using MasterApi.Core.Account.Enums;
using MasterApi.Web.Filters;

namespace MasterApi.Web.Controllers.v1.Admin
{
    /// <summary>
    /// Handles audience related requests
    /// </summary>
    /// <seealso cref="MasterApi.Web.Controllers.BaseController" 
    [Module(Name = ModelType.Audience)]
    [Route("api/{version}/[controller]")]
    [ClaimsAuthorize(ClaimTypes.Role, UserAccessLevel.Admin)]
    public class AudienceController : CrudApiController<Audience, AudienceInput, AudienceOutput>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudienceController" /> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userInfo">The user information.</param>
        public AudienceController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) 
            : base(unitOfWork, userInfo)
        {
            //ActionPolicies.Add(ModelAction.Delete, AccessRole)
        }


        /// <summary>
        /// Returns the notification list filtering criteria.
        /// </summary>
        /// <param name="id">The audience identifier.</param>
        /// <returns></returns>
        protected override Expression<Func<Audience, bool>> GetFilter(object id = null)
        {
            Expression<Func<Audience, bool>> predicate = null;
            if (id != null)
            {
                predicate = n => n.ClientId == (string)id;
            }
            return predicate;
        }

        /// <summary>
        /// Returns the notification list ordering criteria.
        /// </summary>
        /// <returns></returns>
        protected override Func<IQueryable<Audience>, IOrderedQueryable<Audience>> GetOrderBy()
        {
            return q => q.OrderByDescending(x => x.ApplicationType).ThenBy(x => x.Name);
        }

    }

}
