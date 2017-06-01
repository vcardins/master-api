using System.Net;

namespace MasterApi.Web.Extensions
{

    public class BadRequestWithMessageResult : OutputWithMessageResult
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public BadRequestWithMessageResult(string message) : base(message, HttpStatusCode.BadRequest)
        { }
    }
}
