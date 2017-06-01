using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterApi.Core.Common;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Models;
using MasterApi.Core.Services;
using MasterApi.Core.ViewModels;

namespace MasterApi.Services.Domain
{
    public class GeoService : Service<Country>, IGeoService
    {
        public GeoService(IUnitOfWorkAsync unitOfWork)
            : base(unitOfWork)
        {            
        }

        public async Task<PackedList<CountryOutput>> GetCountries(int page, int size)
        {
            var repository = UnitOfWork.RepositoryAsync<Country>();

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

        public async Task<PackedList<LanguageOutput>> GetLanguages(int page, int size)
        {
            var repository = UnitOfWork.RepositoryAsync<Language>();

            var entries = repository.Query().OrderBy(q => q.OrderBy(c => c.Name))
                .SelectPagedAsync<LanguageOutput>(page, size, e => new
                {
                    e.Name,
                    e.Code
                });
            return await entries;
        }

        public async Task<PackedList<ProvinceStateOutput>> GetStates(int page, int size, string iso2)
        {
            var repository = UnitOfWork.RepositoryAsync<ProvinceState>();
            var query = repository.Query(e => e.Iso2 == iso2);

            var entries = query
                .OrderBy(q => q.OrderBy(c => c.Name))
                .SelectPagedAsync<ProvinceStateOutput>(page, size, e => new
                {
                    e.Iso2, e.Code, e.Name
                });
            return await entries;
        }

        public async Task<PackedList<CountryEnabledOutput>> GetEnabledCountries(int page, int size)
        {
            var repository = UnitOfWork.RepositoryAsync<EnabledCountry>();
            var query = repository.Query(e => e.Enabled).Include(c=>c.Country);

            var entries = query
                .OrderBy(q => q.OrderBy(c => c.Iso2))
                .SelectPagedAsync<CountryEnabledOutput>(page, size, e => new
                {
                    e.Iso2,
                    e.Country.Name
                });
            return await entries;
        }

        public async Task EnableDisableCountry(string iso2, int userId, bool enable)
        {
            var repository = UnitOfWork.RepositoryAsync<EnabledCountry>();
            var record = await repository.FirstOrDefaultAsync(c => c.Iso2 == iso2);

            if (record == null)
            {
                if (!enable) return;
                var obj = new EnabledCountry {
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

        /// <summary>
        /// Sets the country langauge.
        /// 1. If there is already a default language setup and the coming is default, set the old as NOT default and the new as default
        /// </summary>
        /// <param name="iso2">The iso2.</param>
        /// <param name="languageCode">The language code.</param>
        /// <param name="main">if set to <c>true</c> [main].</param>
        /// <returns></returns>
        public async Task SetCountryLanguage(string iso2, string languageCode, bool main)
        {
            var repository = UnitOfWork.RepositoryAsync<LanguageCountry>();
            var records = await repository.Query(c => c.Iso2 == iso2).SelectAsync();
            
            var entries = (IList<LanguageCountry>) records;
           
            if (!entries.Any())
            {
                await repository.InsertAsync(new LanguageCountry { Iso2 = iso2, LanguageCode = languageCode, Default = main });
            }
            else
            {
                //Check if the current entry already exist in the database
                var exists = entries.Any(c => c.LanguageCode.Equals(languageCode) && c.Default == main);

                //If it has the same state as the database one, just return
                if (exists) { return; }
            
                var defaultOne = entries.FirstOrDefault(c => c.Default);
                //if there is already a default one
                if (defaultOne != null && main)
                {                    
                    //If the one being set as default is alread the default one, just return
                    if (defaultOne.LanguageCode.Equals(languageCode))
                    {
                        return;
                    }
                    defaultOne.Default = false;
                    await repository.UpdateAsync(defaultOne);    
                }
                //Retrieve the entry being updated by language code
                var record = entries.FirstOrDefault(c => c.LanguageCode == languageCode);
                if (record != null)
                {
                    record.Default = main;
                    await repository.UpdateAsync(record);
                }
                else
                {
                    await repository.InsertAsync(new LanguageCountry { Iso2 = iso2, LanguageCode = languageCode, Default = main });
                }
            }
            UnitOfWork.SaveChanges();
        }
    }

}
