using System.Threading.Tasks;
using MasterApi.Core.Common;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Models;
using MasterApi.Core.ViewModels;
using System.Linq;

namespace MasterApi.Data.Repositories
{
    public static class CountryRepository
    {
        public static async Task<PackedList<CountryOutput>> GetAllCountries(this IRepositoryAsync<Country> repository,
            int page = 0, int size = 0)
        {
            var entries = repository.Query().OrderBy(q => q.OrderBy(c => c.Name))
                .SelectPagedAsync<CountryOutput>(page, size, e => new
                {
                    e.Iso2,
                    e.Iso3,
                    e.Name,
                    e.Currency,
                    e.PhoneCode,
                    e.Latitude,
                    e.Longitude
                });
            return await entries;
        }

        public static async Task<PackedList<CountryEnabledOutput>> GetEnabled(this IRepositoryAsync<EnabledCountry> repository,
           int page = 0, int size = 0)
        {
            var query = repository.Query(e => e.Enabled).Include(c => c.Country);

            var entries = query
                .OrderBy(q => q.OrderBy(c => c.Iso2))
                .SelectPagedAsync<CountryEnabledOutput>(page, size, e => new
                {
                    e.Iso2,
                    e.Country.Name
                });
            return await entries;
        }

        public static async Task EnableDisableCountry(this IRepositoryAsync<EnabledCountry> repository, 
            string iso2, int userId, bool enable)
        {
            var record = await repository.FirstOrDefaultAsync(c => c.Iso2 == iso2);

            if (record == null)
            {
                if (!enable) return;
                var obj = new EnabledCountry
                {
                    Iso2 = iso2,
                    Enabled = true
                };
                await repository.InsertAsync(obj, true);
            }
            else
            {
                record.Enabled = enable;
                await repository.UpdateAsync(record, true);
            }
        }
    }

}