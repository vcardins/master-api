namespace MasterApi.Core.Messaging
{
    /// <summary>
    /// Message Request class
    /// </summary>
    public class MessageRequest
    {

        /// <summary>
        /// Get or set message value
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Get or set EventName
        /// </summary>
        public MessagingEvent EventName { get; set; }

    }
}
