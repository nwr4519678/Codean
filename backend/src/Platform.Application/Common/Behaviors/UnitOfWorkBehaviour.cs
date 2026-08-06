using MediatR;
using Platform.Application.Common.Abstractions;

namespace Platform.Application.Common.Behaviors;

/// <summary>
/// Opens a single SaveChanges() per command (write) request. Read-only
/// requests skip this for performance.
/// </summary>
public sealed class UnitOfWorkBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _uow;
    public UnitOfWorkBehaviour(IUnitOfWork uow) => _uow = uow;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        // Heuristic: anything ending with "Command" is a write.
        var isCommand = name.EndsWith("Command", StringComparison.Ordinal);

        if (isCommand)
        {
            await using var tx = await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var resp = await next();
                await _uow.SaveChangesAsync(cancellationToken);
                await _uow.CommitAsync(cancellationToken);
                return resp;
            }
            catch
            {
                await _uow.RollbackAsync(cancellationToken);
                throw;
            }
        }

        return await next();
    }
}

