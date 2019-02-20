using MasterApi.Core.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MasterApi.Core.Data.Repositories
{
	public interface IRepository<TEntity> where TEntity : class, IObjectState
    {
        long Count(Expression<Func<TEntity, bool>> query = null);
        long TotalCount();
        TEntity Create();       
        TEntity Find(params object[] keyValues);
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> filter);
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        IEnumerable<TEntity> Select(Expression<Func<TEntity, bool>> filter);
        IQueryable FindBy(Expression<Func<TEntity, bool>> filter);
        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);
        Task ExecuteSql(string query, params object[] parameters);
        IEnumerable<TEntity> GetAll();
        int Insert(TEntity entity, bool? commit = false);
        int InsertRange(IEnumerable<TEntity> entities, bool? commit = false);
        int InsertOrUpdateGraph(TEntity entity, bool? commit = false);
        int InsertGraphRange(IEnumerable<TEntity> entities, bool? commit = false);
        int Update(TEntity entity, bool? commit = false);
        int Delete(object id, bool? commit = false);
        int Delete(Expression<Func<TEntity, bool>> predicate);
        int DeleteMany(params object[] parameters);

        int Delete(TEntity entity, bool? commit = false);
        int Commit();
        IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject);
        IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query);
        IQueryFluent<TEntity> Query();
        IQueryable<TEntity> Queryable();
        IRepository<T> GetRepository<T>() where T : class, IObjectState;

        IEnumerable<TEntity> AllIncluding(params Expression<Func<TEntity, object>>[] includeProperties);

        event EventHandler OnEntityCreated;
        event EventHandler OnEntityUpdated;
        event EventHandler OnEntityDeleted;
    }
}