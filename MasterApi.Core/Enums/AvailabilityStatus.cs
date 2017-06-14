using System.ComponentModel;

namespace MasterApi.Core.Enums
{
    public enum AvailabilityStatus
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Online")]
        Online = 1,
        [Description("Away")]
        Away = 2,
        [Description("Do not Disturb")]
        DoNotDisturb = 3,
        [Description("Invisible")]
        Invisible = 4,
        [Description("Offline")]
        Offline = 5
    }
}