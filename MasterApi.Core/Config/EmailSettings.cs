namespace MasterApi.Core.Config
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ApiKey { get; set; }
    }
}
