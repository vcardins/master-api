using System.Collections.Generic;

namespace MasterApi.Core.Models
{
    public class ProvinceState : BaseObjectState
    {
        public string Iso2 { get; set; }
        public Country Country { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserProfile> UserProfiles { get; set; }
    }

}
