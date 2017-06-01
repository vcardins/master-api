using System.Threading.Tasks;
using MasterApi.Core.Common;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Models;
using MasterApi.Core.ViewModels;
using System.Linq;

namespace MasterApi.Data.Repositories
{
    public static class ProvinceStateRepository
    {
        public static async Task<PackedList<ProvinceStateOutput>> GetAllByCountry(this IRepositoryAsync<ProvinceState> repository,
            string iso2, int page = 0, int size = 0)
        {
            var query = repository.Query(e => e.Iso2 == iso2);

            var entries = query
                .OrderBy(q => q.OrderBy(c => c.Name))
                .SelectPagedAsync<ProvinceStateOutput>(page, size, e => new
                {
                    e.Iso2,
                    e.Code,
                    e.Name
                });
            return await entries;
        }
    }

}