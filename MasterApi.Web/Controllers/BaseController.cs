using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Attributes;
using MasterApi.Core.Constants;
using MasterApi.Core.Enums;
using MasterApi.Core.Events;
using MasterApi.Core.Extensions;
using MasterApi.Web.Extensions;
using Microsoft.AspNetCore.SignalR.Hubs;
using MasterApi.Web.SignalR.Hubs;

namespace MasterApi.Web.Controllers
{
    public abstract class BaseController : BaseController<NotificationHub>
    {
        protected BaseController(IUserInfo userInfo) : base(userInfo) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseController<THub> : Controller where THub : IHub
    {
        protected readonly IUserInfo UserInfo;

        protected BaseController(IUserInfo userInfo)
        {
            UserInfo = userInfo;
        }
        
        /// <summary>
        /// 
        /// </summary>
        protected string Controller { get; set; }

        protected string CurrentAction { get; set; }

        private ModelType? _module;

        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        protected ModelType Module
        {
            get
            {                
                if (_module.HasValue) return _module.GetValueOrDefault();
                _module = GetType().GetAttributeValue((ModuleAttribute dna) => dna.Name);
                return  _module.GetValueOrDefault();
            }
            set { _module = value; }
        }

        ///// <summary>
        ///// Adds the header.
        ///// </summary>
        ///// <param name="key">The key.</param>
        ///// <param name="value">The value.</param>
        protected void AddHeader(string key, object value)
        {
            HttpContext.Response.Headers.Add(key, value.ToString());
        }

        ///// <summary>
        ///// Called when [action executing].
        ///// </summary>
        ///// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            Task.Run(() => {
                Controller = actionContext.Controller.ToString();
                CurrentAction = actionContext.ActionDescriptor.DisplayName;
            });
        }

        [HttpPost("lang")]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            return LocalRedirect(returnUrl);
        }

        /// <summary>
        /// Oks the specified event action.
        /// </summary>
        /// <param name="eventAction">The event action.</param>
        /// <param name="eventStatus">The event status.</param>
        /// <param name="data">The data.</param>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        protected IActionResult Ok(ModelAction eventAction, EventStatus eventStatus, object data = null, string msg = null)
        {
            var action = eventAction.GetDescription().ToLower();
            if (string.IsNullOrEmpty(msg)) {
                switch (eventStatus)
                {
                    case EventStatus.Success:
                        msg = string.Format("{0} {1} com sucesso", string.Format("{0}(s)",Module.GetDescription()), action);
                        break;
                    case EventStatus.Failure:
                        msg = string.Format("Um erro ocorreu. {0} não pode ser {1}", Module.GetDescription(), action);
                        break;
                    case EventStatus.Pending:
                        msg = string.Format("{0} ação {1} está pendente no momento", action, Module.GetDescription());
                       break;
                    case EventStatus.NoAction:
                       msg = string.Format("{0} está atualizado(a). Nenhuma ação foi realizada", Module.GetDescription());
                        break;
                }
            }
            var content = data == null ? (object) new {Message = msg} : new {Message = msg, Data = data};
            return Ok(content);
        }

        /// <summary>
        /// Nots the found.
        /// </summary>
        /// <returns></returns>
        protected IActionResult InvalidModel()
        {
            return BadRequest(AppConstants.InformationMessages.InvalidRequestParameters);
        }
     
        /// <summary>
        /// Nots the found.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected IActionResult NotFound(string message = null)
        {
            var msg = message ?? string.Format(AppConstants.InformationMessages.NotFound, Module.GetDescription());
            return new NotFoundWithMessageResult(msg);   
        }

        /// <summary>
        /// Forbiddens the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected IActionResult Forbidden(string message = null)
        {
            var msg = message ?? string.Format(AppConstants.InformationMessages.AccessNotAllowed);
            return new ForbiddenWithMessageResult(msg);   
        }

        /// <summary>
        /// Nots the found.
        /// </summary>
        /// <returns></returns>
        protected IActionResult NotFound(int id)
        {
            return NotFound(string.Format(AppConstants.InformationMessages.NotFound, Module.GetDescription(), id));
        }

        /// <summary>
        /// Bads the request.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected IActionResult BadRequest(string message = null)
        {
            var msg = message ?? string.Format(AppConstants.InformationMessages.BadRequest);
            return new BadRequestWithMessageResult(msg);
        }

        private static void TriggerLiveUpdate(ModelType group, ModelAction action,
            ICollection recipients, object result, int? userId)
        {
            var evt = new DomainEvent(group, action, result) { UserId = userId };
            if (recipients.Count.Equals(0))
            {
                Console.WriteLine(evt);
                //Hub.Clients.All.handleEvent(evt);
            }
            else
            {
                Console.WriteLine(evt);
                //Hub.Clients.Groups(recipients.ToArray()).handleEvent(evt);
            }
        }

        protected void LiveUpdate(ModelType group, ModelAction action, List<string> recipients, object result, int? userId = null)
        {
            TriggerLiveUpdate(group, action, recipients, result, userId);            
        }

        protected void HandleChange(object sender, NotificationEvent e)
        {
            LiveUpdate(ModelType.Notification, ModelAction.Create, new List<string> { e.Username }, e.Event, e.UserId);
        }
    }
}
