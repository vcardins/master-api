using System.Net;

namespace MasterApi.Web.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class NotFoundWithMessageResult : OutputWithMessageResult
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public NotFoundWithMessageResult(string message) : base(message, HttpStatusCode.NotFound)
        { }

    }
}

