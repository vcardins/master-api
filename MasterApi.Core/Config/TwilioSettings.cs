namespace MasterApi.Core.Config
{
    public class TwilioSettings
    {
        public string Sid { get; set; }
        public string Token { get; set; }
        public string BaseUri { get; set; }
        public string RequestUri { get; set; }
        public string From { get; set; }
        public int SmsMaxLength { get; set; }
    }
}
