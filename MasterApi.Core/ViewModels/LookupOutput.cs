using MasterApi.Core.Common;
using System.Collections.Generic;

namespace MasterApi.Core.ViewModels
{
    public class LookupOutput
    {
        public IEnumerable<EnumOutput> AvailabilityStatuses { get; set; }
        public IEnumerable<DateFormatOutput> DateFormats { get; set; }
        public IEnumerable<EnumOutput> DateRanges { get; set; }
        public IEnumerable<CountryOutput> Countries { get; set; }
    }
}
