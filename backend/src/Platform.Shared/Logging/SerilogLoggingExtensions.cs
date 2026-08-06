using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.OpenSearch;

namespace Platform.Shared.Logging;

public static class SerilogLoggingExtensions
{
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration cfg)
    {
        var serviceName = cfg["Service:Name"] ?? "Platform";
        var openSearchUrl = cfg["OpenSearch:Url"];

        var loggerCfg = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("service", serviceName)
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {service} {Message:lj} {Properties:j}{NewLine}{Exception}");

        if (!string.IsNullOrWhiteSpace(openSearchUrl))
        {
            loggerCfg.WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(openSearchUrl))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{serviceName.ToLowerInvariant()}-logs-{{0:yyyy.MM}}",
                MinimumLogEventLevel = LogEventLevel.Information,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog
                                       | EmitEventFailureHandling.WriteToFailureSink,
                FailureCallback = (e) => Console.Error.WriteLine($"OpenSearch emit failed: {e.MessageTemplate}")
            });
        }

        Log.Logger = loggerCfg.CreateLogger();

        services.AddSerilog(dispose: true);
        return services;
    }
}
