using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Common;
using Microsoft.EntityFrameworkCore;
//using Omu.ValueInjecter;

namespace MasterApi.Data.EF7
{
    public sealed class QueryFluent<TEntity> : IQueryFluent<TEntity> 
        where TEntity : class, IObjectState
    {
        #region Private Fields
        private readonly Expression<Func<TEntity, bool>> _expression;
        private readonly List<Expression<Func<TEntity, object>>> _includes;
        private readonly Repository<TEntity> _repository;
        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderBy;
        private int _pageSize = int.MaxValue;
        private int? _page;

        #endregion Private Fields

        #region Constructors
        public QueryFluent(Repository<TEntity> repository)
        {
            _repository = repository;
            _includes = new List<Expression<Func<TEntity, object>>>();
        }

        public QueryFluent(Repository<TEntity> repository, IQueryObject<TEntity> queryObject) : this(repository)
        {
            _expression = queryObject.Query();
        }

        public QueryFluent(Repository<TEntity> repository, Expression<Func<TEntity, bool>> expression)
            : this(repository)
        {
            _expression = expression;
        }
        #endregion Constructors     

        public IQueryFluent<TEntity> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            _orderBy = orderBy;
            return this;
        }

        public IQueryFluent<TEntity> Include(Expression<Func<TEntity, object>> expression)
        {
            if (expression != null)
            {
                _includes.Add(expression);
            }
            return this;
        }

        public IQueryFluent<TEntity> Size(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public IQueryFluent<TEntity> Page(int page)
        {
            _page = page;
            return this;
        }

        public IEnumerable<TEntity> SelectPage(int page, int pageSize)
        {
            return _repository.Select(_expression, _orderBy, _includes, page, pageSize);
        }

        public async Task<IEnumerable<TEntity>> SelectPageAsync(int page, int pageSize)
        {
            return await _repository.SelectAsync(_expression, _orderBy, _includes, page, pageSize);
        }

        public async Task<PackedList<TResult>> SelectPageAsync<TResult>(int page, int pageSize) where TResult : new()
        {
            var entries = await _repository.SelectAsync(_expression, _orderBy, _includes, page, pageSize);
            var result = new PackedList<TResult>
            {
                Data = entries.Select(x => new TResult().InjectFrom(x)).Cast<TResult>()
                //<CloneInjection>
            };            
            return result;
        }

        public IEnumerable<TEntity> Select()
        {
            return _repository.Select(_expression, _orderBy, _includes);
        }

        public async Task<IEnumerable<TEntity>> SelectAsync(Expression<Func<TEntity, object>> selector)
        {
            var query = _repository.Select(_expression, _orderBy, _includes, _page, _pageSize);
            var entries = await query.Select(selector).ToListAsync();
            return entries.Select(x => x).Cast<TEntity>(); 
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> selector)
        {
            return await _repository.FirstOrDefaultAsync(_expression, _includes);
        }

        public IEnumerable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return _repository.Select(_expression, _orderBy, _includes, _page, _pageSize).Select(selector);
        }

        public IEnumerable<TResult> Select<TResult>(Expression<Func<TEntity, object>> selector = null) where TResult : new()
        {
            var query = _repository.Select(_expression, _orderBy, _includes, _page, _pageSize);
            var entries = (selector != null ? query.Select(selector) : query).ToList();
            return entries.Select(x => new TResult().InjectFrom(x)).Cast<TResult>();
            //<CloneInjection>
        }

        public async Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<TEntity, object>> selector) where TResult : new()
        {
            var query = _repository.Select(_expression, _orderBy, _includes, _page, _pageSize);
            var entries = await (selector != null ? query.Select(selector) : query).ToListAsync();
            return entries.Select(x => new TResult().InjectFrom(x)).Cast<TResult>(); //<CloneInjection>
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, object>> selector) where TResult : class, new()
        {
            var query = _repository.Select(_expression, _orderBy, _includes);
            var entry = await (selector != null ? query.Select(selector) : query).FirstOrDefaultAsync();
            if (entry == null) { return null; }
            return new TResult().InjectFrom(entry) as TResult;
        }

        public async Task<PackedList<TResult>> SelectPagedAsync<TResult>(int page, int pageSize, Expression<Func<TEntity, object>> selector = null) where TResult : new()
        {
            var total = await _repository.Select(_expression, _orderBy, _includes).CountAsync();
            _page = page;
            if (pageSize > 0)
            {
                _pageSize = pageSize;
            }
            var entries = await SelectAsync<TResult>(selector);
            return new PackedList<TResult> { Data = entries, Total = total };
        }

        public async Task<PackedList<TResult>> SelectPagedAsync<TResult>(Expression<Func<TEntity, object>> selector = null) where TResult : new()
        {
            var total = await _repository.Select(_expression, _orderBy, _includes).CountAsync();
            var entries = await SelectAsync<TResult>(selector);
            return new PackedList<TResult> { Data = entries, Total = total };
        }

        public async Task<IEnumerable<TEntity>> SelectAsync()
        {
            return await _repository.SelectAsync(_expression, _orderBy, _includes);
        }
      
        public IQueryable<TEntity> SqlQuery(string query, params object[] parameters)
        {
            return _repository.SelectQuery(query, parameters).AsQueryable();
        }

    }
}