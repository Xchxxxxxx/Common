using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Common.EF.EF
{
    public interface IRepository<T, TKey> : IReadOnlyRepository<T, TKey>
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        // Write Operations
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> UpdateBatchAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<int> DeleteBatchAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteAsync(TKey id, string? deletedBy = null, CancellationToken cancellationToken = default);
    }
}
