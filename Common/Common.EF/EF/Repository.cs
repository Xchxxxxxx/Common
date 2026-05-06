using Common.EF.EF.entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Common.EF.EF
{
    public class Repository<T, TKey> : IRepository<T, TKey>
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        #region Read Operations

        public virtual async Task<T?> GetByIdAsync(
            TKey id,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            // 应用 Include（支持 ThenInclude）
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            // 应用 Include（支持 ThenInclude）
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<T?> FindOneAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            // 应用 Include（支持 ThenInclude）
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            // 应用 Include（支持 ThenInclude）
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            return await query.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<T?> GetSingleBySpecAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IReadOnlyList<T>> GetBySpecAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<int> CountAsync(
            ISpecification<T>? spec = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking();

            if (spec != null)
            {
                query = ApplySpecification(spec);
            }

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> AnyAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            return predicate == null
                ? await _dbSet.AnyAsync(cancellationToken).ConfigureAwait(false)
                : await _dbSet.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<PagedResult<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = _dbSet.AsNoTracking();

            // 应用 Include（支持 ThenInclude）
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// 获取可查询对象（支持链式操作和 ThenInclude）
        /// </summary>
        public virtual IQueryable<T> Query(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                query = include(query);
            }

            return query;
        }

        /// <summary>
        /// 获取可跟踪的查询对象（支持 ThenInclude）
        /// </summary>
        public virtual IQueryable<T> TrackedQuery(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = _dbSet.AsTracking();

            foreach (var include in includes)
            {
                query = include(query);
            }

            return query;
        }

        #endregion

        #region Write Operations

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            return entities;
        }

        public virtual Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.FromResult(entity);
        }

        public virtual Task<int> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.UpdateRange(entities);
            return Task.FromResult(entities.Count());
        }

        public virtual async Task<int> UpdateBatchAsync(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, T>> updateExpression,
            CancellationToken cancellationToken = default)
        {
            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);

            if (!entities.Any()) return 0;

            var compiledUpdate = updateExpression.Compile();
            foreach (var entity in entities)
            {
                var updated = compiledUpdate(entity);
                _context.Entry(entity).CurrentValues.SetValues(updated);
            }

            return entities.Count;
        }

        public virtual Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            return Task.FromResult(true);
        }

        public virtual async Task<bool> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            return true;
        }

        public virtual async Task<int> DeleteBatchAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (!entities.Any()) return 0;

            _dbSet.RemoveRange(entities);
            return entities.Count;
        }

        public virtual async Task<bool> SoftDeleteAsync(
            TKey id,
            string? deletedBy = null,
            CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (entity == null) return false;

            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                softDelete.DeletedAt = DateTime.UtcNow;

                if (entity is IAuditableEntity auditable)
                {
                    auditable.UpdatedAt = DateTime.UtcNow;
                    auditable.UpdatedBy = deletedBy;
                }

                await UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        #endregion

        #region Raw SQL

        public virtual async Task<IReadOnlyList<T>> FromSqlRawAsync(
            string sql,
            params object[] parameters)
        {
            return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> ExecuteSqlRawAsync(
            string sql,
            params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters).ConfigureAwait(false);
        }

        #endregion

        #region Private Methods

        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            var query = _dbSet.AsQueryable();

            // 应用 Include
            query = spec.Includes
                .Aggregate(query, (current, include) => current.Include(include));

            query = spec.IncludeStrings
                .Aggregate(query, (current, include) => current.Include(include));

            // 应用筛选条件
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            // 应用排序
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            // 应用分页
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip ?? 0).Take(spec.Take ?? 10);
            }

            return query;
        }

        #endregion
    }
}