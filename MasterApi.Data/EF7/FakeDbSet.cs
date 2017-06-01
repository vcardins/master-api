using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Models;

namespace MasterApi.Data.EF7
{
    public abstract class FakeDbSet<TEntity> : DbSet<TEntity> where TEntity : BaseObjectState, new() //, IDbSet<TEntity>
    {
        #region Private Fields

        private readonly IQueryable _query;
        #endregion Private Fields

        protected FakeDbSet()
        {
            Local = new ObservableCollection<TEntity>();
            _query = Local.AsQueryable();
        }

        //IEnumerator IEnumerable.GetEnumerator() { return _items.GetEnumerator(); }
        public IEnumerator<TEntity> GetEnumerator() { return Local.GetEnumerator(); }

        public Expression Expression => _query.Expression;

        public Type ElementType => _query.ElementType;

        public IQueryProvider Provider => _query.Provider;

        public new TEntity Add(TEntity entity)
        {
            Local.Add(entity);
            return entity;
        }

        public new TEntity Remove(TEntity entity)
        {
            Local.Remove(entity);
            return entity;
        }

        public new TEntity Attach(TEntity entity)
        {
            switch (entity.ObjectState)
            {
                case ObjectState.Modified:
                    Local.Remove(entity);
                    Local.Add(entity);
                    break;

                case ObjectState.Deleted:
                    Local.Remove(entity);
                    break;

                case ObjectState.Unchanged:
                case ObjectState.Added:
                    Local.Add(entity);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return entity;
        }

        public TEntity Create() { return new TEntity(); }

        public TDerivedEntity Create<TDerivedEntity>() { return Activator.CreateInstance<TDerivedEntity>(); }

        public new ObservableCollection<TEntity> Local { get; }
    }
}