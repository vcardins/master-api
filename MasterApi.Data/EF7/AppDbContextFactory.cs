using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MasterApi.Core.Config;

namespace MasterApi.Data.EF7
{
    public class AppDbContextFactory : IDbContextFactory<AppDbContext>
    {
        //private AppSettings _settings;

        //public AppDbContextFactory(IOptions<AppSettings> settings)
        //{
        //    _settings = settings.Value;           
        //}

        public AppDbContext Create(DbContextFactoryOptions options)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            //switch (_settings.DbConnection.InMemoryProvider)
            //{
            //    case true:
            //        builder.UseInMemoryDatabase();
            //        break;
            //    default:
            //        builder.UseSqlServer(_settings.DbConnection.ConnectionString, b => b.MigrationsAssembly("MasterApi.Web"));
            //        break;
            //} , b => b.MigrationsAssembly("MasterApi.Web")
            builder.UseSqlServer("Server=(localdb)\\v11.0;Database=MasterApiDB;Trusted_Connection=True;MultipleActiveResultSets=true");
            return new AppDbContext(builder.Options);
        }
    }
}
