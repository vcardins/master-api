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
    /// Handles Notebooks requests
    /// </summary>
    [Module(Name = ModelType.Notebook)]
    [Route("api/{version}/[controller]")]
    public class NotebookController : CrudApiController<Note, NoteOutput, NoteOutput>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookController" /> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userInfo">The user information.</param>
        public NotebookController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) 
            : base(unitOfWork, userInfo)
        {
        }

        /// <summary>
        /// Returns the notification list filtering criteria.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected override Expression<Func<Note, bool>> GetFilter(object id = null)
        {
            Expression<Func<Note, bool>> predicate = n => n.UserId == UserInfo.UserId;
            if (id!=null)
            {
                predicate = n => n.Id == (int)id;
            }
            return predicate;
        }

        /// <summary>
        /// Returns the notification list ordering criteria.
        /// </summary>
        /// <returns></returns>
        protected override Func<IQueryable<Note>, IOrderedQueryable<Note>> GetOrderBy()
        {
            return q => q.OrderBy(x => x.SortOrder).ThenByDescending(x => x.Created);
        }
    }

}
