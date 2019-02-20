#region
using System.Threading;
using System.Threading.Tasks;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Data.DataContext;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Data.Infrastructure;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Storage;

#endregion

namespace MasterApi.Data.EF7
{
    public class UnitOfWork : IUnitOfWorkAsync
    {
        #region Private Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly IDataContextAsync _dataContext;       
        private readonly IDbContextTransaction _transaction;
        private Dictionary<string, dynamic> _repositories;
        
        #endregion Private Fields

        #region Constuctor/Dispose

        public UnitOfWork(IDataContextAsync dataContext, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dataContext = dataContext;
            _repositories = new Dictionary<string, dynamic>();
        }

        //A public event for listeners to subscribe to
        public event EventHandler OnSaveChanges;

		#endregion Constuctor/Dispose

		public int SaveChanges()
        {
            // Could also be before try if you know the exception occurs in SaveChanges
            var changes = _dataContext.SaveChanges();
            //Fire the event - notifying all subscribers
            OnSaveChanges?.Invoke(this, null);
            return changes;
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IObjectState
        {
            if (_serviceProvider!=null)
            {
                return (IRepository<TEntity>)_serviceProvider.GetService(typeof(IRepository<TEntity>));
            }

            return RepositoryAsync<TEntity>();
        }

        public Task<int> SaveChangesAsync()
        {
            return _dataContext.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dataContext.SaveChangesAsync(cancellationToken);
        }

        public IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class, IObjectState
        {
            if (_serviceProvider != null)
            {
                return (IRepositoryAsync<TEntity>)_serviceProvider.GetService(typeof(IRepositoryAsync<TEntity>));
            }

            if (_repositories == null)
            {
                _repositories = new Dictionary<string, dynamic>();
            }

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
            {
                return (IRepositoryAsync<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(RepositoryAsync<>);

            _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _dataContext, this));

            return _repositories[type];
        }

        #region Unit of Work Transactions

        public void BeginTransaction(DbIsolationLevel isolationLevel = DbIsolationLevel.Unspecified)
        {
            _transaction.GetDbTransaction();
        }

        public bool Commit()
        {
            _transaction.Commit();
            return true;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _dataContext.SyncObjectsStatePostCommit();
        }

        #endregion
    }
}