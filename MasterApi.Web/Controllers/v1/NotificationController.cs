using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Attributes;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Enums;
using MasterApi.Core.Models;
using MasterApi.Core.ViewModels;

namespace MasterApi.Web.Controllers.v1
{
    /// <summary>
    /// Handles Notifications requests
    /// </summary>
    /// <seealso cref="Controllers.CrudApiController{Notification, Core.ViewModels.NotificationOutput, Core.ViewModels.NotificationOutput}" />
    [Module(Name = ModelType.Notification)]
    [Route("api/{version}/[controller]")]
    public class NotificationController : CrudApiController<Notification, NotificationOutput, NotificationOutput>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController" /> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userInfo">The user information.</param>
        public NotificationController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) 
            : base(unitOfWork, userInfo)
        {
        }

        /// <summary>
        /// Returns the notification list filtering criteria.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected override Expression<Func<Notification, bool>> GetFilter(object id = null)
        {
            Expression<Func<Notification, bool>> predicate = n => n.UserId == UserInfo.UserId;
            if (id!=null)
            {
                predicate = n => n.UserId == (int)id;
            }
            return predicate;
        }

        /// <summary>
        /// Returns the notification list ordering criteria.
        /// </summary>
        /// <returns></returns>
        protected override Func<IQueryable<Notification>, IOrderedQueryable<Notification>> GetOrderBy()
        {
            return q => q.OrderByDescending(x => x.Created).ThenBy(x => x.Type);
        }
    }

}
