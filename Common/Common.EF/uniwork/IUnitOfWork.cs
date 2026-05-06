using Common.EF.EF;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.uniwork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T, TKey> Repository<T, TKey>() where T : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<bool> SaveChangesWithRetryAsync(int maxRetries = 3, CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
    }
}
