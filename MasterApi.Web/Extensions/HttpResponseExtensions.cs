using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MasterApi.Web.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method to add pagination info to Response headers
        /// </summary>
        /// <param name="response"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="totalItems"></param>
        /// <param name="totalPages"></param>
        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader));
            // CORS
            response.Headers.Add("access-control-expose-headers", "Pagination");
        }

        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            // CORS
            response.Headers.Add("access-control-expose-headers", "Application-Error");
        }

        public static async Task ExecuteResultAsync(this HttpResponse httpResponse, object response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            // you need to do this before setting the body content
            httpResponse.StatusCode = (int)statusCode;
            httpResponse.ContentType = "application/json";

            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var output = JsonConvert.SerializeObject(response, serializerSettings);
            await httpResponse.WriteAsync(output);
        }

        public static async Task ExecuteErrorAsync(this HttpResponse httpResponse, string description, string error = "", HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (string.IsNullOrEmpty(error))
            {
                await ExecuteResultAsync(httpResponse, new { error_description = description }, statusCode);
            }
            else
            {
                await ExecuteResultAsync(httpResponse, new { error = error, error_description = description }, statusCode);
            }
        }

        public static async Task ExecuteErrorAsync(this HttpResponse httpResponse, object response, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            await ExecuteResultAsync(httpResponse, response, statusCode);
        }
    }
}
