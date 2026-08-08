using System;
using System.Reflection;
using Platform.Domain.Results;

namespace Platform.Application.Common.Behaviors;

public static class ResultHelper
{
    /// <summary>
    /// Creates a typed failure result, handling both Result&lt;T&gt; and non-generic Result.
    /// Prevents InvalidCastException when returning Error in MediatR pipeline behaviors.
    /// </summary>
    public static TResponse CreateFailure<TResponse>(Error error)
    {
        var type = typeof(TResponse);

        // Result<T> — call Result<T>.Failure(error) via reflection
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = type.GetGenericArguments()[0];
            var method = typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod("Failure", BindingFlags.Public | BindingFlags.Static, [typeof(Error)])!;
            return (TResponse)method.Invoke(null, [error])!;
        }

        // Non-generic Result
        if (type == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        throw new InvalidOperationException($"Cannot create failure result for response type {type.FullName}");
    }
}
