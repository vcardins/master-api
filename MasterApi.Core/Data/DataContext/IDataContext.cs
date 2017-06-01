using System;
using MasterApi.Core.Data.Infrastructure;

namespace MasterApi.Core.Data.DataContext
{
    public interface IDataContext : IDisposable
    {
        //IDbTransaction BeginTransaction(DbIsolationLevel isolationLevel);
        int? GetKey<TEntity>(TEntity entity);
        int SaveChanges();
        void SyncObjectState<TEntity>(TEntity entity) where TEntity : class, IObjectState;
        void SyncObjectsStatePostCommit();        
    }
}