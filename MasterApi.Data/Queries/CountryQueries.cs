using MasterApi.Core.Models;
using MasterApi.Data.EF7;

namespace MasterApi.Data.Queries
{
    public class CountryQueries : QueryObject<Country>
    {
        public CountryQueries ByCode(string iso2)
        {
            And(x => x.Iso2 == iso2);
            return this;
        }
    }
}