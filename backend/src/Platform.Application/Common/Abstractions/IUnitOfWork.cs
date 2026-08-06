using System.Linq.Expressions;

namespace Platform.Application.Common.Abstractions;

/// <summary>
/// Repository abstraction that the Application layer uses. Implemented in Infrastructure.
/// </summary>
public interface IRepository<TEntity>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken ct = default) where TId : notnull;
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    IQueryable<TEntity> Query();
}

/// <summary>
/// Coordinates a unit of work - one SaveChanges() per use case, with optional transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}

/// <summary>
/// A vendor-agnostic transaction handle.
/// </summary>
public interface IDbTransaction : IDisposable, IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
