using Newtonsoft.Json;

namespace MasterApi.Web.Identity
{
    /// <summary>
    /// Authentication error response object
    /// </summary>
    public class AuthError
    {
        /// <summary>
        /// Gets or sets the error description.
        /// </summary>
        /// <value>
        /// The error description.
        /// </value>
        [JsonProperty("error_description")]
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
