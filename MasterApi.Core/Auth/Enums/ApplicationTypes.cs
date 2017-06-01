using System.ComponentModel;

namespace MasterApi.Core.Auth.Enums
{
    public enum ApplicationTypes
    {
        [Description("Native")]
        NativeConfidential = 0,

        [Description("JavaScript")]
        JavaScript = 1
    }
}