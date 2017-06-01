using System.ComponentModel;

namespace MasterApi.Core.Enums
{
    public enum ModelType
    {
        [Description("Application Settings")]
        AppSettings = 0,
        [Description("Application Audience")]
        Audience = 1,
        [Description("Users")]
        User = 2,
        [Description("Exception")]
        ExceptionLog = 90,
        [Description("General")]
        Unknown = 100,
        EnabledCountry,
        CountryLanguage,
        Country,
        Notification
    }
}
