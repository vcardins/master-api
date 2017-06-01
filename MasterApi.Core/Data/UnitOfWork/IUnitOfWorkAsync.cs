using System.Threading;
using System.Threading.Tasks;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Data.Repositories;

namespace MasterApi.Core.Data.UnitOfWork
{
    public interface IUnitOfWorkAsync : IUnitOfWork
    {
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class, IObjectState;
    }
}