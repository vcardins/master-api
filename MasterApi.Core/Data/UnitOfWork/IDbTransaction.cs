using System;

namespace MasterApi.Core.Data.UnitOfWork
{
    public interface IDbTransaction : IDisposable
    {
        void BeginTransaction(DbIsolationLevel isolationLevel = DbIsolationLevel.Unspecified);
        bool Commit();
        void Rollback();
    }
}