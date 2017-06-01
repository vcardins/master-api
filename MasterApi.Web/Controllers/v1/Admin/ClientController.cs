using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Attributes;
using MasterApi.Core.Auth.Models;
using MasterApi.Core.Auth.ViewModel;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Enums;

namespace MasterApi.Web.Controllers.v1.Admin
{
    [Module(Name = ModelType.Audience)]
    [Route("api/{version}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ClientController : CrudApiController<Audience, AudienceInput, AudienceOutput>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientController" /> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userInfo">The user information.</param>
        public ClientController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) 
            : base(unitOfWork, userInfo)
        {
            //ActionPolicies.Add(ModelAction.Delete, AccessRole)
        }

        protected override Expression<Func<Audience, bool>> GetFilter(object id = null)
        {
            Expression<Func<Audience, bool>> predicate = null;
            if (id != null)
            {
                predicate = n => n.ClientId == (string)id;
            }
            return predicate;
        }

        protected override Func<IQueryable<Audience>, IOrderedQueryable<Audience>> GetOrderBy()
        {
            return q => q.OrderByDescending(x => x.ApplicationType).ThenBy(x => x.Name);
        }

    }

}
