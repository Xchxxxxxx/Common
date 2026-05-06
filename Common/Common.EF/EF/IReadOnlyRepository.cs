using Common.EF.EF.entity;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Common.EF.EF
{
    public interface IReadOnlyRepository<T, TKey>
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        // Read Operations with ThenInclude support
        Task<T?> GetByIdAsync(
            TKey id,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        Task<IReadOnlyList<T>> GetAllAsync(
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        Task<T?> FindOneAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        Task<IReadOnlyList<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        // Specification pattern
        Task<T?> GetSingleBySpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetBySpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

        // Count and existence
        Task<int> CountAsync(ISpecification<T>? spec = null, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        // Paging with ThenInclude support
        Task<PagedResult<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        // Query builder with ThenInclude support
        IQueryable<T> Query(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);
        IQueryable<T> TrackedQuery(params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes);

        // Raw SQL
        Task<IReadOnlyList<T>> FromSqlRawAsync(string sql, params object[] parameters);
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
    }
}
