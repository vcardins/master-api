using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Attributes;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Enums;
using MasterApi.Core.Models;
using MasterApi.Web.ViewModels;

namespace MasterApi.Web.Controllers.v1
{
    [Module(Name = ModelType.User)]
    [Route("api/{version}/[controller]")]
    public class UserController : CrudApiController<UserProfile, UserViewModel, UserViewModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserController" /> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userInfo">The user information.</param>
        public UserController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) 
            : base(unitOfWork, userInfo)
        {
        }

        protected override Expression<Func<UserProfile, bool>> GetFilter(object id = null)
        {
            Expression<Func<UserProfile, bool>> predicate = n => n.UserId == UserInfo.UserId;
            if (id!=null)
            {
                predicate = n => n.UserId == (int)id;
            }
            return predicate;
        }

        protected override Func<IQueryable<UserProfile>, IOrderedQueryable<UserProfile>> GetOrderBy()
        {
            return q => q.OrderByDescending(x => x.Created).ThenBy(x => x.FirstName);
        }
    }

}
