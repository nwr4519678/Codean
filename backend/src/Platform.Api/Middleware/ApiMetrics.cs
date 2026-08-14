using System.Text;

namespace Platform.Api.Middleware;

public static class ApiMetrics
{
    private static long _requests;
    private static long _errors;

    public static void RecordRequest() => Interlocked.Increment(ref _requests);
    public static void RecordError() => Interlocked.Increment(ref _errors);

    public static string Snapshot()
    {
        var builder = new StringBuilder();
        builder.AppendLine("# HELP platform_http_requests_total Total HTTP requests observed by the API.");
        builder.AppendLine("# TYPE platform_http_requests_total counter");
        builder.AppendLine($"platform_http_requests_total {Interlocked.Read(ref _requests)}");
        builder.AppendLine("# HELP platform_http_errors_total Total unhandled HTTP errors observed by the API.");
        builder.AppendLine("# TYPE platform_http_errors_total counter");
        builder.AppendLine($"platform_http_errors_total {Interlocked.Read(ref _errors)}");
        return builder.ToString();
    }
}
