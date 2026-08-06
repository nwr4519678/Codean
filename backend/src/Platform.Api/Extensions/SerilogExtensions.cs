using Microsoft.Extensions.Hosting;
using Serilog;

namespace Platform.Api.Extensions;

/// <summary>
/// Configures Serilog for the Platform API host.
/// Full configuration (sinks, enrichers, output template) is centralized here.
/// Values are read from appsettings.json Serilog section at runtime.
/// Called from Program.cs: builder.Host.AddPlatformSerilog()
/// </summary>
public static class SerilogExtensions
{
    public static IHostBuilder AddPlatformSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((ctx, services, config) => config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));
    }
}
