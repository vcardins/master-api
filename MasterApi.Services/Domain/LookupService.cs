using MasterApi.Core.Common;
using MasterApi.Core.Enums;
using MasterApi.Core.Extensions;
using MasterApi.Core.Infrastructure.Caching;
using MasterApi.Core.Services;
using MasterApi.Core.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterApi.Services.DomainServices
{
    public class LookupService : ILookupService
    {
        private readonly IMemoryCache _cache;
        private readonly IGeoService _geoService;
        private static MemoryCacheEntryOptions _cacheOptions;

        public LookupService(IGeoService geoService, IMemoryCache cache)
        {
            _geoService = geoService;
            _cache = cache;
            _cacheOptions = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove,
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };
        }

        public async Task<IEnumerable<CountryOutput>> GetCountries(int page = 0, int size = 0)
        {
            PackedList<CountryOutput> result;
            if (page > 0 && size > 0)
            {
                result = await _geoService.GetCountries(page, size);
            }
            else
            {
                if (!_cache.TryGetValue(DataCacheKey.Countries, out result))
                {
                    result = await _geoService.GetCountries(page, size);
                    _cache.Set(DataCacheKey.Countries, result, _cacheOptions);
                }
            }
            return result.Data;
        }

        public IEnumerable<DateFormatOutput> GetDateFormats()
        {
            return new List<DateFormatOutput>
            {
                new DateFormatOutput { Format = "h:mm tt", Name = "" },
                new DateFormatOutput { Format = "M/d/yyyy", Name = "" },
                new DateFormatOutput { Format = "h:mm:ss tt", Name = "" },
                new DateFormatOutput { Format = "dddd, MMMM dd, yyyy", Name = "" },
                new DateFormatOutput { Format = "dddd, MMMM dd, yyyy h:mm tt", Name = "" },
                new DateFormatOutput { Format = "M/d/yyyy h:mm tt", Name = "" },
                new DateFormatOutput { Format = "M/d/yyyy h:mm:ss tt", Name = "" },
                new DateFormatOutput { Format = "MMMM dd", Name = "" },
                new DateFormatOutput { Format = "MMMM, yyyy", Name = "" },
                new DateFormatOutput { Format = "M/d/yyyy", Name = "" },
                new DateFormatOutput { Format = "MM/dd/yyyy", Name = "" },
                new DateFormatOutput { Format = "ddd, MMM d, yyyy", Name = "" },
                new DateFormatOutput { Format = "dddd, MMMM d, yyyy", Name = "" },
                new DateFormatOutput { Format = "MM/dd/yy", Name = "" },
                new DateFormatOutput { Format = "MM/dd/yyyy", Name = "" }
            };
        }

        public IEnumerable<EnumOutput> GetEnumOutput(Enum e)
        {
            var values = Enum.GetValues(e.GetType()).Cast<object>();
            var models = values.Select(v => new EnumOutput
            {
                Id = (int)v, 
                Name = (v as Enum).GetDescription() ?? v.ToString()
            }).ToList();
            return models;
        }

        
        public async Task<LookupOutput> GetAll()
        {

            var models = new LookupOutput
            {
                AvailabilityStatuses = GetEnumOutput(AvailabilityStatus.Undefined),
                DateFormats = GetDateFormats(),
                DateRanges = GetEnumOutput(DateRange.All),
                Countries = await GetCountries(),
            };
            
            return models;
        }
      
    }

}
