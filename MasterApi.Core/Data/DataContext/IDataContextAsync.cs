using System.Threading;
using System.Threading.Tasks;

namespace MasterApi.Core.Data.DataContext
{
    public interface IDataContextAsync : IDataContext
    {
        //Task BeginTransactionAsync(DbIsolationLevel isolationLevel);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
        Task SyncObjectsStatePostCommitAsync();
        Task ExecuteSqlAsync(string query, CancellationToken cancellationToken, params object[] parameters);
    }
}