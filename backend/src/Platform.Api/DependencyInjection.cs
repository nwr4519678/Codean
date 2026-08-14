using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Platform.Api.Authorization;
using Platform.Api.OpenApi;
using Platform.Application.Common.Settings;
using Platform.Infrastructure.Configuration;
using Platform.Infrastructure.Security;

namespace Platform.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddPlatformOpenApi();

        // ── CORS ────────────────────────────────────────────────────────────
        services.AddCors(options =>
        {
            options.AddPolicy("PlatformCors", policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // ── Options ─────────────────────────────────────────────────────────
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // LockoutSettings lives in Application (business rule) — bound here by the host
        services.Configure<LockoutSettings>(configuration.GetSection(LockoutSettings.SectionName));

        services.Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName));
        services.Configure<EmailVerificationOptions>(configuration.GetSection(EmailVerificationOptions.SectionName));
        services.Configure<PasswordResetOptions>(configuration.GetSection(PasswordResetOptions.SectionName));

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development" ||
                          (!string.IsNullOrWhiteSpace(o.Secret) || o.SigningKeys.Any(k => !string.IsNullOrWhiteSpace(k.Secret))),
                "A production JWT signing secret or signing key is required.")
            .ValidateOnStart();

        // ── JWT Authentication ───────────────────────────────────────────────
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                         ?? new JwtOptions();

        // Build signing keys from config (supports multi-key rotation)
        var signingKeys = jwtOptions.SigningKeys
            .Where(k => !string.IsNullOrWhiteSpace(k.Secret))
            .Select(k => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(k.Secret)))
            .Cast<SecurityKey>()
            .ToList();

        // Fallback to legacy Secret if no SigningKeys configured
        if (signingKeys.Count == 0 && !string.IsNullOrWhiteSpace(jwtOptions.Secret))
            signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = signingKeys,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // JTI Blacklist check via JwtBearerEvents (no middleware needed)
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var jtiClaim = ctx.Principal?.FindFirst("jti")
                                       ?? ctx.Principal?.FindFirst(
                                           System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti);

                        if (jtiClaim is null)
                        {
                            ctx.Fail("Token missing JTI claim.");
                            return;
                        }

                        var blacklist = ctx.HttpContext.RequestServices
                            .GetRequiredService<ITokenBlacklistService>();

                        var isBlacklisted = await blacklist.IsBlacklistedAsync(
                            jtiClaim.Value, ctx.HttpContext.RequestAborted);

                        if (isBlacklisted)
                            ctx.Fail("Token has been revoked.");
                    }
                };
            });

        // ── Authorization ────────────────────────────────────────────────────
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization();

        // ── Rate Limiting ────────────────────────────────────────────────────
        services.AddRateLimiter(opts =>
        {
            opts.AddFixedWindowLimiter("auth_login", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 30;
                o.QueueLimit = 0;
            });
            opts.AddFixedWindowLimiter("auth_register", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 50;
                o.QueueLimit = 0;
            });
            opts.AddFixedWindowLimiter("auth_sensitive", o =>
            {
                o.Window = TimeSpan.FromMinutes(15);
                o.PermitLimit = 5;
                o.QueueLimit = 0;
            });
            opts.AddFixedWindowLimiter("code_submission", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 10;
                o.QueueLimit = 0;
            });
            opts.AddFixedWindowLimiter("payment_checkout", o =>
            {
                o.Window = TimeSpan.FromMinutes(15);
                o.PermitLimit = 5;
                o.QueueLimit = 0;
            });
            opts.AddFixedWindowLimiter("api_read_general", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 100;
                o.QueueLimit = 10;
            });

            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // ── Hangfire (host responsibility) ───────────────────────────────────
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(o =>
                o.UseNpgsqlConnection(configuration.GetConnectionString("Hangfire")
                    ?? configuration.GetConnectionString("DefaultConnection")!)));

        services.AddHangfireServer(opts =>
        {
            opts.WorkerCount = 4;
            opts.Queues = ["default", "critical"];
        });

        return services;
    }
}
