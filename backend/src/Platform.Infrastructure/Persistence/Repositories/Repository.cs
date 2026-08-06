using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Common.Abstractions;
using Platform.Infrastructure.Persistence.Context;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository backed by EF Core. Works with any POCO entity.
/// </summary>
public sealed class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly AppDbContext _db;

    public Repository(AppDbContext db) => _db = db;

    public async Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
        where TId : notnull =>
        await _db.Set<TEntity>().FindAsync([id], ct);

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        _db.Set<TEntity>().FirstOrDefaultAsync(predicate, ct);

    public async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default) =>
        predicate is null
            ? await _db.Set<TEntity>().ToListAsync(ct)
            : await _db.Set<TEntity>().Where(predicate).ToListAsync(ct);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default) =>
        predicate is null
            ? _db.Set<TEntity>().CountAsync(ct)
            : _db.Set<TEntity>().CountAsync(predicate, ct);

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) =>
        _db.Set<TEntity>().AnyAsync(predicate, ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        await _db.Set<TEntity>().AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) =>
        await _db.Set<TEntity>().AddRangeAsync(entities, ct);

    public void Update(TEntity entity) => _db.Set<TEntity>().Update(entity);

    public void Remove(TEntity entity) => _db.Set<TEntity>().Remove(entity);

    public IQueryable<TEntity> Query() => _db.Set<TEntity>().AsQueryable();
}
