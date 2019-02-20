using System;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Data.Repositories;

namespace MasterApi.Core.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        event EventHandler OnSaveChanges;
        int SaveChanges();
        IRepository<TEntity> Repository<TEntity>() where TEntity : class, IObjectState;
        void BeginTransaction(DbIsolationLevel isolationLevel = DbIsolationLevel.Unspecified);
        bool Commit();
        void Rollback();
    }
}