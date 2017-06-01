
namespace MasterApi.Core.Config
{
    public class DbConnection
    {
        public string ConnectionString { get; set; }

        public bool InMemoryProvider { get; set; }
    }
}
