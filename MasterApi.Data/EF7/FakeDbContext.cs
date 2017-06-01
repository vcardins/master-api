using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MasterApi.Core.Data.DataContext;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Models;

namespace MasterApi.Data.EF7
{
    public interface IFakeDbContext : IDataContextAsync
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        void AddFakeDbSet<TEntity, TFakeDbSet>() where TEntity : BaseObjectState, new()
            where TFakeDbSet : FakeDbSet<TEntity>, new(); //, IDbSet<TEntity>
    }

    public abstract class FakeDbContext : IFakeDbContext
    {
        #region Private Fields  
        private readonly Dictionary<Type, object> _fakeDbSets;
        #endregion Private Fields

        protected FakeDbContext()
        {
            _fakeDbSets = new Dictionary<Type, object>();
        }

        public int? GetKey<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int SaveChanges() { return default(int); }

        public void SyncObjectState<TEntity>(TEntity entity) where TEntity : class, IObjectState
        {
            // no implentation needed, unit tests which uses FakeDbContext since there is no actual database for unit tests, 
            // there is no actual DbContext to sync with, please look at the Integration Tests for test that will run against an actual database.
        }

        void IDataContext.SyncObjectsStatePostCommit()
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) { return new Task<int>(() => default(int)); }

        public Task<int> SaveChangesAsync() { return new Task<int>(() => default(int)); }
        public Task SyncObjectsStatePostCommitAsync()
        {
            throw new NotImplementedException();
        }

        public Task ExecuteSqlAsync(string query, params object[] parameters)
        {
            return Task.FromResult(0);
        }

        public void Dispose() { }

        public DbSet<T> Set<T>() where T : class { return (DbSet<T>)_fakeDbSets[typeof(T)]; }

        public void AddFakeDbSet<TEntity, TFakeDbSet>()
            where TEntity : BaseObjectState, new()
            where TFakeDbSet : FakeDbSet<TEntity>, new() //, IDbSet<TEntity>
        {
            var fakeDbSet = Activator.CreateInstance<TFakeDbSet>();
            _fakeDbSets.Add(typeof(TEntity), fakeDbSet);
        }

        public Task SyncObjectsStatePostCommit()
        {
            throw new NotImplementedException();
        }

        public Task ExecuteSqlAsync(string query, CancellationToken cancellationToken, params object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}