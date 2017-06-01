using System.Net;

namespace MasterApi.Web.Extensions
{

    public class ForbiddenWithMessageResult : OutputWithMessageResult
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ForbiddenWithMessageResult(string message) : base(message, HttpStatusCode.Forbidden)
        { }
    }
}
