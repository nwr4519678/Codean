using Microsoft.EntityFrameworkCore;
using Platform.Api;
using Platform.Api.Extensions;
using Platform.Api.Middleware;
using Platform.Api.OpenApi;
using Platform.Application;
using Platform.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Starting Platform API host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddPlatformSerilog();

    builder.Services.AddPlatformApplication();
    builder.Services.AddPlatformInfrastructure(builder.Configuration);
    builder.Services.AddPlatformApi(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UsePlatformMiddlewares();

    if (app.Environment.IsDevelopment())
        app.UsePlatformOpenApi();

    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.UseCors("PlatformCors");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapPlatformHealthChecks();
    app.MapPlatformHangfire();
    app.RegisterPlatformJobs();

    // ── Migrate & Seed ────────────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db     = scope.ServiceProvider.GetRequiredService<Platform.Infrastructure.Persistence.Context.AppDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var hasher = scope.ServiceProvider.GetService<Platform.Application.Common.Contracts.Authentication.IPasswordHasher>();

        // Startup migration is opt-in for hosted environments. Free Render web
        // services do not support paid pre-deploy commands, so the API performs
        // the migration once during startup when explicitly enabled.
        if (builder.Configuration.GetValue("RunMigrationsOnStartup", app.Environment.IsDevelopment()))
            await db.Database.MigrateAsync();

        // Seed core reference data (Roles etc.). Development admin seeding is
        // explicitly configurable so local runs against shared external
        // databases do not create development credentials there.
        var seedDevelopmentAdmin = builder.Configuration.GetValue(
            "SeedDevelopmentAdmin", app.Environment.IsDevelopment());
        await Platform.Infrastructure.Persistence.DatabaseSeeder.SeedAsync(db, seeder, seedDevelopmentAdmin, hasher);
    }

    app.Run();

}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Platform API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
