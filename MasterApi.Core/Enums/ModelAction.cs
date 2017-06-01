using System.ComponentModel;

namespace MasterApi.Core.Enums
{
    public enum ModelAction
    {
        [Description("Created")]
        Create,
        [Description("Read")]
        Read,
        [Description("Updated")]
        Update,
        [Description("Deleted")]
        Delete,
        [Description("Liked")]
        Like,
        [Description("UnLiked")]
        UnLike,
        [Description("Commented")]
        Comment,
        [Description("Connected")]
        Connect,
        [Description("Disconnected")]
        Disconnect,
        [Description("Status Changed")]
        StatusChange,
        [Description("Joined")]
        Join,
        [Description("Message Sent")]
        MessageSent,
        [Description("Message Received")]
        MessageReceived,
        [Description("Typed")]
        Typing,
        [Description("Rated")]
        Rate,
        [Description("Linked")]
        Link,
        [Description("Unlinked")]
        Unlink,
        [Description("Imported")]
        Import,
        [Description("Approved")]
        Approve,
        [Description("Canceled")]
        Cancel,
        [Description("Requested")]
        Request,
        [Description("Rejected")]
        Reject,
        [Description("Exported")]
        Export,
        [Description("Published")]
        Publish, 
        [Description("Custom")]
        Custom = 99,
        [Description("Unknown")]
        Unknown = 100,
        [Description("Unknown")]
        BackgroundTask
    }
}