using System.Collections.Generic;

namespace MasterApi.Core.Models
{
    public class Country : BaseObjectState
    {
        public string Iso2 { get; set; }
        public string Iso3 { get; set; }
        public int NumericCode { get; set; }
        public string Name { get; set; }
        public string OficialName { get; set; }
        public string Capital { get; set; }
        public string Currency { get; set; }
        public string PhoneCode { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string PostalCodeFormat { get; set; }
        public string PostalCodeRegex { get; set; }
        public virtual ICollection<LanguageCountry> Languages { get; set; }
        public virtual ICollection<UserProfile> UserResidences { get; set; }
        public virtual ICollection<ProvinceState> ProvinceStates { get; set; }
        public virtual EnabledCountry EnabledCountry { get; set; }
    }
}
