namespace MasterApi.Core.Config
{
    public class DbSettings
    {
        public string ConnectionString { get; set; }
        public bool InMemoryProvider { get; set; }
        public DbSettings()
        {
            InMemoryProvider = false;
        }
    }
}
