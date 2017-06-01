using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MasterApi.Web.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class OutputWithMessageResult : IActionResult
    {
        private readonly HttpStatusCode _httpStatusCode;
        private readonly string _message;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpStatusCode"></param>
        protected OutputWithMessageResult(string message, HttpStatusCode httpStatusCode)
        {
            _httpStatusCode = httpStatusCode;
            _message = message;
        }

        public virtual async Task ExecuteResultAsync(ActionContext context)
        {
            // you need to do this before setting the body content
            context.HttpContext.Response.StatusCode = (int)_httpStatusCode;
            context.HttpContext.Response.ContentType = "application/json";

            var output = JsonConvert.SerializeObject(
                new { Message = _message }, 
                new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                );
            var myByteArray = Encoding.UTF8.GetBytes(output);
            
            await context.HttpContext.Response.Body.WriteAsync(myByteArray, 0, myByteArray.Length);
            await context.HttpContext.Response.Body.FlushAsync();
        }
    }
}

