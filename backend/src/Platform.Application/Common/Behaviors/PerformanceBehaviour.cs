using MediatR;
using Microsoft.Extensions.Logging;

namespace Platform.Application.Common.Behaviors;

/// <summary>
/// Warns if a handler exceeds the 500 ms performance budget.
/// </summary>
public sealed class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
    private const int ThresholdMs = 500;

    public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var resp = await next();
        sw.Stop();
        if (sw.ElapsedMilliseconds > ThresholdMs)
            _logger.LogWarning("Slow handler {Request} took {Ms} ms (threshold {Threshold} ms)",
                typeof(TRequest).Name, sw.ElapsedMilliseconds, ThresholdMs);
        return resp;
    }
}

