using System.Threading.Tasks;
using MasterApi.Core.Common;
using MasterApi.Core.Models;
using MasterApi.Core.ViewModels;

namespace MasterApi.Core.Services
{
    public interface IGeoService : IService<Country>
    {
        Task<PackedList<CountryOutput>> GetCountries(int page, int size);
        Task<PackedList<LanguageOutput>> GetLanguages(int page, int size);
        Task<PackedList<ProvinceStateOutput>> GetStates(int page, int size, string iso2);
        Task EnableDisableCountry(string iso2, int userId, bool enable);
        Task SetCountryLanguage(string iso2, string languageCode, bool main);
        Task<PackedList<CountryEnabledOutput>> GetEnabledCountries(int page, int size);
    }
}
