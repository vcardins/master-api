using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.AspNetCore.Http.Features;

//http://stackoverflow.com/questions/38357108/form-submit-resulting-in-invaliddataexception-form-value-count-limit-1024-exce
namespace MasterApi.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestFormSizeLimitAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        private readonly FormOptions _formOptions;

        public RequestFormSizeLimitAttribute(int valueCountLimit)
        {
            _formOptions = new FormOptions
            {
                ValueCountLimit = valueCountLimit
            };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var features = context.HttpContext.Features;
            var formFeature = features.Get<IFormFeature>();

            if (formFeature?.Form == null)
            {
                // Request form has not been read yet, so set the limits
                features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, _formOptions));
            }
        }
    }
}
