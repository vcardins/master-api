#region
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Data.DataContext;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Filters;

#endregion

namespace MasterApi.Data.EF7
{
    public class Repository<TEntity> : IRepositoryAsync<TEntity> where TEntity : class, IObjectState
    {
        #region Private Fields

        private readonly IDataContextAsync _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly IUnitOfWorkAsync _unitOfWork;

        public event EventHandler OnEntityCreated;
        public event EventHandler OnEntityUpdated;
        public event EventHandler OnEntityDeleted;

        private readonly ConcurrentQueue<EventHandler> _events = new ConcurrentQueue<EventHandler>();


        #endregion Private Fields

        public Repository(IDataContextAsync context, IUnitOfWorkAsync unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;

           
            // Temporarily for FakeDbContext, Unit Test and Fakes
            var dbContext = context as DbContext;

            if (dbContext != null)
            {
                _dbSet = dbContext.Set<TEntity>();
            }
            else
            {
                var fakeContext = context as FakeDbContext;

                if (fakeContext != null)
                {
                    _dbSet = fakeContext.Set<TEntity>();
                }
            }

            unitOfWork.OnSaveChanges += HandleSaveChanges;

            if (OnEntityCreated != null) { _events.Enqueue(OnEntityCreated); }
            if (OnEntityUpdated != null) { _events.Enqueue(OnEntityUpdated); }
            if (OnEntityDeleted != null) { _events.Enqueue(OnEntityDeleted); }
           
        }

        public void HandleSaveChanges(object sender, EventArgs e)
        {
            EventHandler dispatch;
            while (_events.TryDequeue(out dispatch))
            {
                dispatch(this, e);
            }
        }


        public long TotalCount()
        {
            return _dbSet.LongCount();
        }

        public virtual async Task<long> TotalCountAsync()
        {
            return await _dbSet.LongCountAsync();
        }
     
        public long Count(Expression<Func<TEntity, bool>> query)
        {
            return query != null ? _dbSet.Count(query) : _dbSet.Count();
        }

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> query)
        {
            return await _dbSet.CountAsync(query);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return _dbSet.AsEnumerable();
        }

        public virtual IEnumerable<TEntity> AllIncluding(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _dbSet;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query.AsEnumerable();
        }

        public virtual TEntity Create()
        {
            throw new NotImplementedException(); //return _dbSet.Create();
        }

        public virtual TEntity Find(params object[] keyValues)
        {
            return _dbSet.Find(keyValues);
        }

        public virtual IQueryable FindBy(Expression<Func<TEntity, bool>> filter)
        {
            return _dbSet.Where(filter);
        }

        public virtual IEnumerable<TEntity> Select(Expression<Func<TEntity, bool>> filter)
        {
            return _dbSet.Where(filter).ToList();
        }

        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> filter)
        {
            return _dbSet.FirstOrDefault(filter);
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _dbSet;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query.Where(predicate).FirstOrDefault();
        }

        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
        {
            return null;// _dbSet.SqlQuery(query, null, parameters).AsQueryable();
        }

        public virtual async Task ExecuteSql(string query, params object[] parameters)
        {
            var cancellationToken = new CancellationToken(); //, cancellationToken
            await _context.ExecuteSqlAsync(query, cancellationToken, parameters);
        }
       
        private void _Insert(TEntity entity)
        {
            entity.ObjectState = ObjectState.Added;
            _dbSet.Add(entity);
            _context.SyncObjectState(entity);            
        }

        public virtual int Insert(TEntity entity, bool? commit = false)
        {
            _Insert(entity);
            return commit.GetValueOrDefault() ? _unitOfWork.SaveChanges() : 0;
        }

        public async Task<int> InsertAsync(TEntity entity, bool? commit = false)
        {
            _Insert(entity);
            return commit.GetValueOrDefault() ? await _unitOfWork.SaveChangesAsync() : 0;
        }

        public virtual int InsertRange(IEnumerable<TEntity> entities, bool? commit = false)
        {
            foreach (var entity in entities)
            {
                Insert(entity);
            }
            return commit.GetValueOrDefault() ? _unitOfWork.SaveChanges() : 0;
        }

        public virtual int InsertGraphRange(IEnumerable<TEntity> entities, bool? commit = false)
        {
            _dbSet.AddRange(entities);
            return commit.GetValueOrDefault() ? _unitOfWork.SaveChanges() : 0;
        }
        public virtual int InsertOrUpdateGraph(TEntity entity, bool? commit = false)
        {
            SyncObjectGraph(entity);
            _entitesChecked = null;
            _dbSet.Attach(entity);
            return commit.GetValueOrDefault() ? _unitOfWork.SaveChanges() : 0;
        }

        private void _Update(TEntity entity)
        {
            entity.ObjectState = ObjectState.Modified;
            _dbSet.Attach(entity);
            _context.SyncObjectState(entity);
        }

        public virtual int Update(TEntity entity, bool? commit = false)
        {
            _Update(entity);
            return commit.GetValueOrDefault() ? _unitOfWork.SaveChanges() : 0;
        }

        public async Task<int> UpdateAsync(TEntity entity, bool? commit = false)
        {
            _Update(entity);
            return commit.GetValueOrDefault() ? await _unitOfWork.SaveChangesAsync() : 0;
        }

        public virtual int Delete(object id, bool? commit = false)
        {
            var entity = _dbSet.Find(id);
            return Delete(entity, commit);
        }

        public int DeleteMany(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<int> DeleteManyAsync(List<int> parameters)
        {
            var tableName = typeof(TEntity).Name;
            var sql = string.Format("DELETE FROM [{0}] WHERE Id IN ({1})", tableName, string.Join(", ", parameters.ToArray()));
            await ExecuteSql(sql);
            return 0;
        }

        public virtual int Delete(TEntity entity, bool? commit = false)
        {
            if (entity == null) return 0;
            entity.ObjectState = ObjectState.Deleted;
            _dbSet.Attach(entity);
            _context.SyncObjectState(entity);
            return commit.GetValueOrDefault() ? _unitOfWork.SaveChanges() : 0;
        }

        public virtual int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = FindBy(predicate);
            var counter = 0;
            foreach (var entity in entities)
            {
                counter += Delete(entity);
            }
            return counter;
        }

        public virtual int Commit()
        {
            return _unitOfWork.SaveChanges();  
        }

        public virtual async Task<bool> DeleteAsync(bool? commit = false, params object[] keyValues)
        {
            var rsp = await DeleteAsync(CancellationToken.None, keyValues);
            if (commit.GetValueOrDefault()) { await _unitOfWork.SaveChangesAsync(); }
            return rsp;
        }

        public async Task<bool> DeleteAsync(TEntity entity, bool? commit = false)
        {
            entity.ObjectState = ObjectState.Deleted;
            _dbSet.Attach(entity);
            if (commit.GetValueOrDefault()) { return await _unitOfWork.SaveChangesAsync() > 0; }
            return true;
        }

        public virtual async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> filter, bool? commit = false)
        {
            var entity = await FirstOrDefaultAsync(filter);
            if (entity == null)
            {
                throw new NotFoundException("Record not found or access denied");
            }
            entity.ObjectState = ObjectState.Deleted;
            _dbSet.Attach(entity);
            if (commit.GetValueOrDefault()) { return await _unitOfWork.SaveChangesAsync() > 0; }
            return true;
        }


        public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            var entity = await FindAsync(cancellationToken, keyValues);
            if (entity == null) {
                return false;
            }
            entity.ObjectState = ObjectState.Deleted;
            _dbSet.Attach(entity);
            return true;
        }

        public IQueryFluent<TEntity> Query()
        {
            return new QueryFluent<TEntity>(this);
        }

        public virtual IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject)
        {
            return new QueryFluent<TEntity>(this, queryObject);
        }

        public virtual IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            return new QueryFluent<TEntity>(this, query);
        }

        public IQueryable<TEntity> Queryable()
        {
            return _dbSet;
        }

        public IRepository<T> GetRepository<T>() where T : class, IObjectState
        {
            return _unitOfWork.Repository<T>();
        }

        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }

        public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return await _dbSet.FindAsync(cancellationToken, keyValues);
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(filter);
            return entity;
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, List<Expression<Func<TEntity, object>>> includes)
        {
            var query = _dbSet.Where(filter);
            foreach (var includeProperty in includes)
            {
                query = query.Include(includeProperty);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<TEntity> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> filter, List<Expression<Func<TEntity, object>>> includes)
        {
            var query = _dbSet.Where(filter);
            foreach (var includeProperty in includes)
            {
                query = query.Include(includeProperty);
            }
            return await query.FirstOrDefaultAsync();
        }

        internal IQueryable<TEntity> Select(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy == null) return query;
            query = orderBy(query);
            if (!page.HasValue || !pageSize.HasValue) return query;
            if (!(page > 0) || !(pageSize > 0)) return query;
            var skip = (page.Value - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
            return query;
        }

        internal IQueryable<TEntity> Select(
           List<Expression<Func<TEntity, bool>>> filters = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           List<Expression<Func<TEntity, object>>> includes = null,
           int? page = null,
           int? pageSize = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (filters != null)
            {
                query = filters.Aggregate(query, (current, filter) => current.Where(filter));                
            }

            if (orderBy == null) return query;
            query = orderBy(query);
            if (!page.HasValue || !pageSize.HasValue) return query;
            if (page > 0 && pageSize > 0)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }

        internal async Task<IEnumerable<TEntity>> SelectAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null,
            Expression<Func<TEntity, object>> projection = null)
        {
            var entries = Select(filter, orderBy, includes, page, pageSize);
            return await entries.ToListAsync();
        }


        private HashSet<object> _entitesChecked; // tracking of all process entities in the object graph when calling SyncObjectGraph

        private void SyncObjectGraph(object entity) // scan object graph for all 
        {
            if(_entitesChecked == null)
                _entitesChecked = new HashSet<object>();

            if (_entitesChecked.Contains(entity))
                return;

            _entitesChecked.Add(entity);

            var objectState = entity as IObjectState;

            if (objectState != null && objectState.ObjectState == ObjectState.Added)
                _context.SyncObjectState((IObjectState)entity);

            // Set tracking state for child collections
            foreach (var prop in entity.GetType().GetProperties())
            {
                // Apply changes to 1-1 and M-1 properties
                var trackableRef = prop.GetValue(entity, null) as IObjectState;
                if (trackableRef != null)
                {
                    if (trackableRef.ObjectState == ObjectState.Added)
                    {
                        _context.SyncObjectState((IObjectState)entity);
                    }
                    SyncObjectGraph(prop.GetValue(entity, null));
                }

                // Apply changes to 1-M properties
                var items = prop.GetValue(entity, null) as IEnumerable<IObjectState>;
                if (items == null) continue;

                Debug.WriteLine("Checking collection: " + prop.Name);

                foreach (var item in items)
                    SyncObjectGraph(item);
            }
        }

    }
}