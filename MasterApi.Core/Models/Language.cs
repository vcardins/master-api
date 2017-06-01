using System.Collections.Generic;

namespace MasterApi.Core.Models
{
    public class Language : BaseObjectState
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public virtual ICollection<LanguageCountry> Countries { get; set; }
    }
}
