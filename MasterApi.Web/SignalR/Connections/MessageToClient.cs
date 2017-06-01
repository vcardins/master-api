namespace MasterApi.Web.SignalR.Connections
{
    public class MessageToClient
    {
        public MessageToClient(string method, string content)
        {
            Method = method;
            Content = content;
        }

        public string Method { get; set; }
        public string Content { get; set; }
    }
}