namespace MasterApi.Core.Models
{
    public class LanguageCountry : BaseObjectState
    {
        public string Iso2 { get; set; }
        public Country Country { get; set; }
        public string LanguageCode { get; set; }
        public Language Language { get; set; }
        public bool Default { get; set; }
    }

}
