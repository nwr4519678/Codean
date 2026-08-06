using MediatR;
using Microsoft.Extensions.Logging;
using Platform.Application.Common;
using Platform.Domain.Results;
using Platform.Domain.Exceptions;

namespace Platform.Application.Common.Behaviors;

/// <summary>
/// Logs request start/finish and duration for every MediatR request.
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Handling {Request} {@Request}", name, request);
        try
        {
            var response = await next();
            sw.Stop();
            _logger.LogInformation("Handled {Request} in {Elapsed} ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error handling {Request} after {Elapsed} ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}

/// <summary>
/// Catches unhandled exceptions and returns a Result.Failure instead of propagating.
/// </summary>
public sealed class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger;
    public UnhandledExceptionBehaviour(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception in {Request}: {Code}", typeof(TRequest).Name, ex.Error.Code);
            return (TResponse)(object)Result.Failure(ex.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in {Request}", typeof(TRequest).Name);
            return (TResponse)(object)Result.Failure(Error.Internal("server.unhandled", "An unexpected error occurred."));
        }
    }
}

