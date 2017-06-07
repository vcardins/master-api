using System.ComponentModel;

namespace MasterApi.Core.Enums
{
    public enum ModelType
    {
        [Description("Application Settings")]
        AppSettings = 0,
        [Description("Application Audience")]
        Audience = 1,
        [Description("User Profile")]
        UserProfile = 2,
        [Description("User Avatar")]
        UserAvatar = 3,
        [Description("Enabled Country")]
        EnabledCountry,
        [Description("Country Language")]
        CountryLanguage,
        [Description("Country")]
        Country,
        [Description("Notification")]
        Notification,
        [Description("Exception")]
        ExceptionLog = 90,
        [Description("General")]
        Unknown = 100,
    }
}
