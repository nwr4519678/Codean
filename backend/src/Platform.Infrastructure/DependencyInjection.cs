using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Common.Contracts.LiveSessions;
using Platform.Application.Common.Contracts.Notifications;
using Platform.Application.Common.Contracts.Payments;
using Platform.Application.Common.Contracts.Search;
using Platform.Application.Common.Contracts.Storage;
using Platform.Infrastructure.Authentication.Jwt;
using Platform.Infrastructure.Authentication.Passwords;
using Platform.Infrastructure.Caching.Hybrid;
using Platform.Infrastructure.HealthChecks;
using Platform.Infrastructure.Identity;
using Platform.Infrastructure.Jobs;
using Platform.Infrastructure.Judge;
using Platform.Infrastructure.LiveSessions.GoogleMeet;
using Platform.Infrastructure.LiveSessions.MicrosoftTeams;
using Platform.Infrastructure.Messaging;
using Platform.Infrastructure.Notifications.Email;
using Platform.Infrastructure.Payments.Paymob;
using Platform.Infrastructure.Persistence.Context;
using Platform.Infrastructure.Persistence.Interceptors;
using Platform.Infrastructure.Persistence.Repositories;
using Platform.Infrastructure.Search.Elasticsearch;
using Platform.Infrastructure.Security;
using Platform.Infrastructure.Storage.CloudflareR2;
using Platform.Infrastructure.Time;

namespace Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Core ─────────────────────────────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // ── Persistence (PostgreSQL) ──────────────────────────────────────────
        services.AddSingleton<OutboxInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
            options
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(sp.GetRequiredService<OutboxInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // ── Messaging (Outbox) ────────────────────────────────────────────────
        // EventDispatcher is internal — registered within Infrastructure, never leaked
        services.AddScoped<EventDispatcher>();
        services.AddScoped<OutboxProcessor>();

        // Jobs (implementations only — scheduling registration is in Platform.Api)
        services.AddScoped<ProcessOutboxJob>();
        services.AddScoped<TokenCleanupJob>();

        // ── Authentication & Security ─────────────────────────────────────────
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenIssuer, JwtTokenService>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

        // ── Caching ───────────────────────────────────────────────────────────
        services.AddHybridCache();
        services.AddScoped<ICacheService, HybridCacheService>();

        // ── Storage ───────────────────────────────────────────────────────────
        services.AddScoped<IObjectStorage, CloudflareR2Storage>();
        services.Configure<R2Options>(configuration.GetSection("R2"));

        // ── Search ────────────────────────────────────────────────────────────
        services.AddScoped<ISearchService, ElasticsearchService>();
        services.Configure<ElasticsearchOptions>(configuration.GetSection("Elasticsearch"));

        // ── Notifications ─────────────────────────────────────────────────────
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IPushSender, WebPushSender>();
        services.AddScoped<ISmsSender, NoopSmsSender>();
        services.AddScoped<IWhatsAppSender, NoopWhatsAppSender>();
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        // ── Live Sessions ─────────────────────────────────────────────────────
        services.AddScoped<IGoogleMeetProvider, GoogleMeetProvider>();
        services.AddHttpClient<IMicrosoftTeamsProvider, MicrosoftTeamsProvider>();
        services.Configure<GoogleMeetOptions>(configuration.GetSection("GoogleMeet"));
        services.Configure<MicrosoftTeamsOptions>(configuration.GetSection("MicrosoftTeams"));

        // ── Payments ──────────────────────────────────────────────────────────
        services.AddHttpClient<IPaymobClient, PaymobClient>();
        services.Configure<PaymobOptions>(configuration.GetSection("Paymob"));

        // ── Judge Service ─────────────────────────────────────────────────────
        services.Configure<JudgeOptions>(configuration.GetSection(JudgeOptions.SectionName));
        services.AddHttpClient<Platform.Application.Common.Contracts.Judge.IJudgeService, OnlineCompilerClient>(
            (sp, client) =>
            {
                var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JudgeOptions>>().Value;
                client.BaseAddress = new System.Uri(opts.BaseUrl);
                client.Timeout     = System.TimeSpan.FromSeconds(opts.TimeoutSeconds);
                if (!string.IsNullOrWhiteSpace(opts.ApiKey))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", opts.ApiKey);
            });

        // ── Health Check Implementations ──────────────────────────────────────
        // Implementations registered here; endpoint mapping is in Platform.Api
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"])
            .AddCheck<CacheHealthCheck>("cache",       tags: ["ready"])
            .AddCheck<SmtpHealthCheck>("smtp",         tags: ["ready"]);

        return services;
    }
}
