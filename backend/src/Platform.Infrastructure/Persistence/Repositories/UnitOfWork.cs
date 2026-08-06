using Microsoft.EntityFrameworkCore.Storage;
using Platform.Application.Common.Abstractions;
using Platform.Infrastructure.Persistence.Context;

namespace Platform.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(AppDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        _tx = await _db.Database.BeginTransactionAsync(ct);
        return new EfTransaction(_tx);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_tx is null) return;
        await _tx.CommitAsync(ct);
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_tx is null) return;
        await _tx.RollbackAsync(ct);
        await _tx.DisposeAsync();
        _tx = null;
    }
}

internal sealed class EfTransaction : IDbTransaction
{
    private readonly IDbContextTransaction _tx;
    public EfTransaction(IDbContextTransaction tx) => _tx = tx;

    public Task CommitAsync(CancellationToken ct = default) => _tx.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => _tx.RollbackAsync(ct);
    public void Dispose() => _tx.Dispose();
    public ValueTask DisposeAsync() => _tx.DisposeAsync();
}
