using System;

namespace MasterApi.Core.Models
{
    public class EnabledCountry : AuditableEntity
    {
        public string Iso2 { get; set; }
        public virtual Country Country { get; set; }
        public bool Enabled { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
