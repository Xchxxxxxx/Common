using Common.EF;
using Common.EF.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.uniwork
{
    public class UnitOfWork<T> : IUnitOfWork where T : DbContext
    {
        private readonly T _context;  // 改为 T 类型
        private readonly ConcurrentDictionary<(Type, Type), object> _repositories;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed;

        // 修改构造函数：接收 T 而不是 DbContext
        public UnitOfWork(T context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new ConcurrentDictionary<(Type, Type), object>();
        }

        public IRepository<TEntity, TKey> Repository<TEntity, TKey>()
            where TEntity : class, IEntity<TKey>
            where TKey : IEquatable<TKey>
        {
            var key = (typeof(TEntity), typeof(TKey));

            return (IRepository<TEntity, TKey>)_repositories.GetOrAdd(key, _ => new Repository<TEntity, TKey>(_context));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities();
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> SaveChangesWithRetryAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
        {
            var retries = 0;

            while (retries < maxRetries)
            {
                try
                {
                    await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    retries++;
                    if (retries == maxRetries) throw;
                }
            }

            return false;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null) throw new InvalidOperationException("No transaction has been started.");

            try
            {
                await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
        {
            await using var transaction = await BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var result = await action().ConfigureAwait(false);
                await CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        private void UpdateAuditableEntities()
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditableEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IAuditableEntity)entry.Entity;
                var now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                }

                entity.UpdatedAt = now;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
                _currentTransaction?.Dispose();
            }
            _disposed = true;
        }
    }
}