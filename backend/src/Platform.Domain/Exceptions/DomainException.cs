using System;
using Platform.Domain.Results;

namespace Platform.Domain.Exceptions;

/// <summary>
/// Base exception for invariant violations. Most use cases prefer
/// <see cref="Result{T}"/> over throwing; this is reserved for truly
/// exceptional paths where a programming error has occurred.
/// </summary>
public class DomainException : Exception
{
    public Error Error { get; }

    public DomainException(Error error)
        : base(error.Message)
    {
        Error = error;
    }

    public DomainException(Error error, Exception inner)
        : base(error.Message, inner)
    {
        Error = error;
    }
}

public sealed class ConcurrencyException : DomainException
{
    public ConcurrencyException()
        : base(Error.Conflict("concurrency.conflict", "The record was modified by another process.")) { }
}

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key)
        : base(Error.NotFound("not.found", $"{entity} '{key}' was not found.")) { }
}
