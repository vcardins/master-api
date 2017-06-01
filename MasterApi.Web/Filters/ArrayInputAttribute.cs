using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace MasterApi.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ArrayInputAttribute : ActionFilterAttribute
    {
        private readonly string _parameterName;
        private readonly Type _parameterType;
        public char Separator { get; set; }

        public ArrayInputAttribute(string parameterName, Type parameterType)
        {
            _parameterName = parameterName;
            _parameterType = parameterType;
            Separator = ',';
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var queryString = actionContext.HttpContext.Request.QueryString.Value;
            var qs = QueryHelpers.ParseQuery(queryString);

            string parameters;
            if (actionContext.RouteData.Values.ContainsKey(_parameterName)) { 
                parameters = (string)actionContext.RouteData.Values[_parameterName];
            }
            else if (qs.ContainsKey(_parameterName))
            {
                parameters = qs[_parameterName];
            }
            else
            {
                return;
            }
            
            if (_parameterType == typeof(int))
                actionContext.ActionArguments[_parameterName] = parameters.Split(Separator).Select(int.Parse).ToList();
            else
                actionContext.ActionArguments[_parameterName] = parameters.Split(Separator).ToList();
        }

        

    }
}
