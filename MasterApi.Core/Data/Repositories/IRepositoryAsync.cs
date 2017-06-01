using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MasterApi.Core.Data.Infrastructure;

namespace MasterApi.Core.Data.Repositories
{
    public interface IRepositoryAsync<TEntity> : IRepository<TEntity> where TEntity : class, IObjectState
    {
        Task<long> CountAsync(Expression<Func<TEntity, bool>> filter);
        Task<long> TotalCountAsync();
        Task<TEntity> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> filter, List<Expression<Func<TEntity, object>>> includes = null);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, List<Expression<Func<TEntity, object>>> includes);
        Task<TEntity> FindAsync(params object[] keyValues);
        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);
        Task<int> InsertAsync(TEntity entity, bool? commit = false);
        Task<int> UpdateAsync(TEntity entity, bool? commit = false);
        Task<bool> DeleteAsync(bool? commit = false, params object[] keyValues);
        Task<bool> DeleteAsync(TEntity entity, bool? commit = false);
        Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> filter, bool? commit = false);
        Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues);
        Task<int> DeleteManyAsync(List<int> parameters);        
    }
}