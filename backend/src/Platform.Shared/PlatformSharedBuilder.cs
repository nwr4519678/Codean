using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Shared.Exceptions;
using Platform.Shared.Results;

namespace Platform.Shared.Common;

/// <summary>
/// Common DI extensions usable by every layer. Keeps registration of
/// primitive abstractions (Result factories, default error mappings)
/// consistent across services.
/// </summary>
public static class PlatformSharedBuilder
{
    /// <summary>
    /// Registers <see cref="Result"/>, <see cref="Result{T}"/>, and the
    /// default exception → error mapping helper used by behaviours and
    /// controllers. Call this once per composition root.
    /// </summary>
    public static IServiceCollection AddPlatformShared(this IServiceCollection services)
    {
        // Result and Error are value/record types; no DI needed. We do register
        // a small factory delegate to keep call sites terse.
        services.TryAddSingleton<DomainExceptionMapper>();
        return services;
    }
}

/// <summary>
/// Maps known domain exceptions to typed <see cref="Error"/>s. Handlers and
/// behaviours call <see cref="Map"/> instead of building Error instances
/// themselves so codes stay consistent.
/// </summary>
public sealed class DomainExceptionMapper
{
    public Error Map(Exception ex) => ex switch
    {
        ConcurrencyException => Error.Conflict("concurrency.conflict", "The record was modified by another process."),
        NotFoundException => Error.NotFound("not.found", ex.Message),
        OperationCanceledException => new Error("cancelled", "The operation was cancelled.", ErrorType.Failure),
        DomainException de => de.Error,
        _ => Error.Internal("server.unhandled", "An unexpected error occurred.")
    };
}
