using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Common;
using MasterApi.Core.Constants;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Enums;
using MasterApi.Core.Extensions;
using MasterApi.Web.Filters;
using MasterApi.Core.Account.Enums;

namespace MasterApi.Web.Controllers
{

    public abstract class CrudApiController<TDomainModel> : CrudApiController<TDomainModel, TDomainModel, TDomainModel>
        where TDomainModel : class, IObjectState, new()
    {
        protected CrudApiController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) : base(unitOfWork, userInfo) { }
    }

    public abstract class CrudApiController<TDomainModel, TOutputViewModel> : CrudApiController<TDomainModel, TOutputViewModel, TOutputViewModel>
        where TDomainModel : class, IObjectState, new()
        where TOutputViewModel : class, new()
    {
        protected CrudApiController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) : base(unitOfWork, userInfo) { }
    }

    public abstract class CrudApiController<TDomainModel, TOutputViewModel, TInputViewModel> : CrudApiController<TDomainModel, TOutputViewModel, TInputViewModel, TInputViewModel>
        where TDomainModel : class, IObjectState, new()
        where TOutputViewModel : class, new()
        where TInputViewModel : class, new()
    {
        protected CrudApiController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) : base(unitOfWork, userInfo) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class CrudApiController<TDomainModel, TOutputViewModel, TInputViewModel, TUpdateViewModel> : BaseController
        where TDomainModel : class, IObjectState, new()
        where TOutputViewModel : class, new()
        where TInputViewModel : class, new()
        where TUpdateViewModel : class, new()
    {

        protected readonly IUnitOfWorkAsync Uow;
        protected readonly IRepositoryAsync<TDomainModel> Repository;
        protected Dictionary<ModelAction, UserAccessLevel> ActionPolicies = new Dictionary<ModelAction, UserAccessLevel>();
        protected bool CollectionExportMode = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrudApiController{TDomainModel, TOutputViewModel, TInputViewModel, TUpdateViewModel}"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userInfo">The user information.</param>
        protected CrudApiController(IUnitOfWorkAsync unitOfWork, IUserInfo userInfo) : base(userInfo)
         {
            Uow = unitOfWork;
            Repository = unitOfWork.RepositoryAsync<TDomainModel>();
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected abstract Expression<Func<TDomainModel, bool>> GetFilter(object id = null);

        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <returns></returns>
        protected abstract Func<IQueryable<TDomainModel>, IOrderedQueryable<TDomainModel>> GetOrderBy();

        // Ts: aggregates the many little media lists in one payload
        // to reduce roundtrips when the client launches.
        // GET: api/medias
        /// <summary>
        /// Gets the medias.
        /// </summary> [FromUri] 
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get(int page = 0, int size = 0)
        {
            var result = await GetPaged(page, size);
            AddHeader("X-TOTAL-RECORDS", result.Total);
            return Ok(result.Data);
        }

        protected virtual async Task<PackedList<TOutputViewModel>> GetPaged(int page = 0, int size = 0)
        {
            var query = Repository.Query(GetFilter()).OrderBy(GetOrderBy());
            var result = await query.SelectPagedAsync<TOutputViewModel>(page, size);
            return result;
        }

        #region
        /**
         * @api {get} /api/client/{id} Retrieve client by Id
         * @apiName GetById
         * @apiGroup Client
         * @apiDescription Returns the user indicated by the provided {id}
         * @apiVersion 2.0.0
         *
         * @apiParam {Int} id Client Id
         * @apiUse ClientObject
         */
        #endregion
        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Route("{id:int}")]
        [HttpGet]
        [ActionName("GetById")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var entity = await Repository.FirstOrDefaultAsync(GetFilter(id));
            if (entity == null) return NotFound(id);
            var model = new TOutputViewModel().InjectFrom(entity);
            return Ok(model);
        }

        #region
        /**
         * @api {post} /api/client Create Client
         * @apiName PostCreate
         * @apiGroup Client
         * @apiDescription Creates a new goal record
         * @apiVersion 2.0.0
         * @apiParam {object} model Client input model.
         * @apiParamExample {json} Request-Example:
         *      {  
         *          parentId : 15,
         *          managerId : 2,
         *          name : "Ryan Schmukler"      
         *      }
         * @apiSuccess {Object} data Client object
         * @apiSuccessExample Success-Response:
         *  HTTP/1.1 201 Created  
         */
        #endregion
        /// <summary>
        /// Posts the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [ActionName("Create")]
        [HttpPost("")]
        [ModelStateValidator]
        //[Log(Action = ModelAction.Create)]
        public virtual async Task<IActionResult> Post(TInputViewModel model)
        {

            if (ActionPolicies.TryGetValue(ModelAction.Create, out UserAccessLevel accessLevel))
            {
                UserInfo.ValidateClaim(ClaimTypes.Role, new[] { accessLevel.ToString() });
            }

            var entity = new TDomainModel();
            entity.InjectFrom(new NullableInjection(new []{"Created"}), model);
            
            var props = entity.GetType().GetProperties();
            var tp = props.SingleOrDefault(x => x.Name == "CreatedById");
            tp?.SetValue(entity, UserInfo.UserId);

            await Repository.InsertAsync(entity, true);

            var result = new TOutputViewModel().InjectFrom<NullableInjection>(entity);

            return Ok(ModelAction.Create, EventStatus.Success, result);

        }

        /// <summary>
        /// Puts the specified model.
        /// </summary>
        /// <param name="id">The T identifier.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        //[Log(Action = ModelAction.Update)]
        [ActionName("Update")]
        [ModelStateValidator]
        public virtual async Task<IActionResult> Put(int id, TUpdateViewModel model)
        {
            if (ActionPolicies.TryGetValue(ModelAction.Update, out UserAccessLevel accessLevel))
            {
                UserInfo.HasClaimValue(ClaimTypes.Role, accessLevel.ToString());
            }

            if (id <= 0) { return BadRequest(AppConstants.InformationMessages.InvalidRequestParameters); }

            var entity = await Repository.FirstOrDefaultAsync(GetFilter(id));
            if (entity == null) { return NotFound(id); }


            entity.InjectFrom(new LoopInjection(new[] { "Id", "Created" }), model);
            
            var props = entity.GetType().GetProperties();
            var tp = props.SingleOrDefault(x => x.Name == "UpdatedById");
            tp?.SetValue(entity, UserInfo.UserId);

            return await UpdateEntity(entity);
        }

        /// <summary>
        /// Puts the specified model.
        /// </summary>
        /// <param name="id">The T identifier.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPatch("{id:int}")]
        //[Log(Action = ModelAction.Update)]
        [ActionName("Patch")]
        public virtual async Task<IActionResult> Patch(int id, IDictionary<string, object> model)
        {
            if (ActionPolicies.TryGetValue(ModelAction.Update, out UserAccessLevel accessLevel))
            {
                UserInfo.ValidateClaim(ClaimTypes.Role, new[] { accessLevel.ToString() });
            }

            if (id <= 0 || !model.Keys.Any())
            {
                return BadRequest(AppConstants.InformationMessages.InvalidRequestParameters);
            }

            var entity = await Repository.FindAsync(id); // .FirstOrDefaultAsync(GetFilter(id));
            if (entity == null) { return NotFound(id); }

            entity.InjectFrom(new DictionaryInjection(new[] { "Id", "Code", "Created" }), model);

            var props = entity.GetType().GetProperties();
            var tp = props.SingleOrDefault(x => x.Name == "UpdatedById");
            tp?.SetValue(entity, UserInfo.UserId);

            return await UpdateEntity(entity);
        }

        private async Task<IActionResult> UpdateEntity(TDomainModel entity)
        {
            await Repository.UpdateAsync(entity, true);
            var result = new TOutputViewModel().InjectFrom<NullableInjection>(entity);
            return Ok(ModelAction.Update, EventStatus.Success, result);
        }

        /// <summary>
        /// Deletes a T by its specified identifier.
        /// </summary>
        /// <param name="id">The T identifier.</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        //[Log(Action = ModelAction.Delete)]
        [ActionName("DeleteSingle")]
        public virtual async Task<IActionResult> DeleteSingle(int id)
        {
            if (ActionPolicies.TryGetValue(ModelAction.Delete, out UserAccessLevel accessLevel))
            {
                UserInfo.ValidateClaim(ClaimTypes.Role, new[] { accessLevel.ToString() });
            }

            var success = await Repository.DeleteAsync(GetFilter(id), true);

            return Ok(ModelAction.Delete, success ? EventStatus.Success : EventStatus.Failure);
        }

        /// <summary>
        /// Deletes a T by its specified identifier.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        [HttpDelete("")]
        [ArrayInput("ids", typeof(int))]
        //[Log(Action = ModelAction.Delete)]
        [ActionName("DeleteMany")]
        public virtual async Task<IActionResult> DeleteMany(List<int> ids)
        {
            if (ids == null)
            {
                return InvalidModel();
            }

            if (ActionPolicies.TryGetValue(ModelAction.Delete, out UserAccessLevel accessLevel))
            {
                UserInfo.ValidateClaim(ClaimTypes.Role, new[] { accessLevel.ToString() });
            }

            await Repository.DeleteManyAsync(ids);
            
            return Ok(ModelAction.Delete, EventStatus.Success);
        }
        
    }

}
