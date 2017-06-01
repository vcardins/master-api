using System;
using Microsoft.AspNetCore.Authorization;
using MasterApi.Core.Account.Enums;

namespace MasterApi.Web.Filters
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string _action;
        private readonly string[] _resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsAuthorizeAttribute"/> class.
        /// </summary>
        public ClaimsAuthorizeAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="resources">The resources.</param>
        public ClaimsAuthorizeAttribute(string action, params string[] resources)
        {
            _action = action;
            _resources = resources;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsAuthorizeAttribute" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="role">The role.</param>
        public ClaimsAuthorizeAttribute(string action, UserAccessLevel role)
        {
            _action = action;
            _resources = new[] { role.ToString() };
        }

        //protected override bool IsAuthorized(ActionContext actionContext)
        //{
        //    if (string.IsNullOrWhiteSpace(_action))
        //    {
        //        return true; //CheckAccess(actionContext);
        //    }
        //    var identity = (ClaimsIdentity)Thread.CurrentThread. .CurrentPrincipal.Identity;
        //    var claim = identity.Claims.FirstOrDefault(c => c.Type == _action && _resources.Contains(c.Value));
        //    if (claim == null)
        //    {
        //        actionContext.Response = actionContext.Request.CreateResponse(
        //            HttpStatusCode.Forbidden,
        //            new { Message = AppConstants.InformationMessages.AccessNotAllowed }
        //        );
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// Checks the access.
        ///// </summary>
        ///// <param name="actionContext">The action context.</param>
        ///// <returns></returns>
        //protected virtual bool CheckAccess(ActionContext actionContext)
        //{
        //    var action = actionContext.ActionDescriptor.ActionName;
        //    var resource = actionContext.ControllerContext.ControllerDescriptor.ControllerName;

        //    return ClaimsAuthorization.CheckAccess(action, resource);
        //}
    }

}