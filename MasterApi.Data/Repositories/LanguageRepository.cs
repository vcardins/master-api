using System.Collections.Generic;
using System.Threading.Tasks;
using MasterApi.Core.Common;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Models;
using MasterApi.Core.ViewModels;
using System.Linq;

namespace MasterApi.Data.Repositories
{
    public static class LanguageRepository
    {
        public static async Task<PackedList<LanguageOutput>> GetAll(this IRepositoryAsync<Language> repository,
            int page = 0, int size = 0)
        {
            var entries = repository.Query().OrderBy(q => q.OrderBy(c => c.Name))
                .SelectPagedAsync<LanguageOutput>(page, size, e => new
                {
                    e.Name,
                    e.Code
                });
            return await entries;
        }

        public static async Task LinkCountryLanguage(this IRepositoryAsync<LanguageCountry> repository,
            string iso2, string languageCode, bool main)
        {
            var records = await repository.Query(c => c.Iso2 == iso2).SelectAsync();

            var entries = (IList<LanguageCountry>)records;

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
                    await repository.UpdateAsync(record, true);
                }
                else
                {
                    await repository.InsertAsync(new LanguageCountry { Iso2 = iso2, LanguageCode = languageCode, Default = main }, true);
                }
            }
        }
    }

}