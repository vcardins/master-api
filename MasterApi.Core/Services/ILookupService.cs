using System.Collections.Generic;
using System.Threading.Tasks;
using MasterApi.Core.ViewModels;

namespace MasterApi.Core.Services
{
    public interface ILookupService
    {
        IEnumerable<DateFormatOutput> GetDateFormats();
        Task<LookupOutput> GetAll();
    }
}
