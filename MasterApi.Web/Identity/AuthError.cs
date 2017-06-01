using Newtonsoft.Json;

namespace MasterApi.Web.Identity
{
    public class AuthError
    {
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
