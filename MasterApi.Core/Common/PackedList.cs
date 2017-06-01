using System.Collections.Generic;

namespace MasterApi.Core.Common
{
    public class PackedList<T>
    {
        public IEnumerable<T> Data { get; set; }
        public long Total { get; set; }
    }
}