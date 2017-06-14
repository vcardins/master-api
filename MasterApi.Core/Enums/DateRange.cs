using System.ComponentModel;

namespace MasterApi.Core.Enums
{
    public enum DateRange
    {
        [Description("Custom")]
        Custom = 0,

        [Description("Last 24 Hours")]
        Last24Hours = 1,

        [Description("Today")]
        Today = 2,

        [Description("Yesterday")]
        Yesterday = 3,

        [Description("Last Week")]
        LastWeek = 4,

        [Description("This Week")]
        ThisWeek = 5,

        [Description("Last Month")]
        LastMonth = 6,

        [Description("All Times")]
        All = 7
    }
}