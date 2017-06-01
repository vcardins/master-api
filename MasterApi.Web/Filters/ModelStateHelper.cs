using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace MasterApi.Web.Filters
{
    public static class ModelStateHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string[]>> Errors(this ModelStateDictionary modelState)
        {
            if (modelState.IsValid) return null;

            var errors = modelState.ToDictionary(kvp => kvp.Key.Replace("model.", ""), kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray())
                .Where(m => m.Value.Any());

            return errors;
        }
    }
}
