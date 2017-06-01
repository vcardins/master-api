namespace MasterApi.Core.Account.Enums
{
    public enum MobileState
    {
        //[Description("The app has not been launched or was running but was terminated by the system.")]
        NotRunning = 0,
        //[Description("The app is running in the foreground but is currently not receiving events. An app usually stays in this state only briefly as it transitions to a different state.")]
        Inactive = 1,
        //[Description("The app is running in the foreground and is receiving events. This is the normal mode for foreground apps.")]
        Active = 2,
        //[Description("The app is in the background and executing code")]
        Background = 3,
        //[Description("The app is in the background but is not executing code")]
        Suspended = 4
    }
}
