using System;
using System.Linq;
using System.Text;
using System.Security.Claims;
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
using Platform.Api.Authentication;
using Platform.Api.OpenApi;
using Platform.Application.Common.Settings;
using Platform.Infrastructure.Configuration;
using Platform.Infrastructure.Security;
using Platform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Platform.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            throw new InvalidOperationException(
                "Required configuration 'ConnectionStrings:DefaultConnection' is missing.");
        }

        var hangfireConnection = configuration.GetConnectionString("Hangfire");
        if (string.IsNullOrWhiteSpace(hangfireConnection))
            hangfireConnection = defaultConnection;

        services.AddControllers();
        services.AddHttpClient<SupabaseAdminClient>();
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
        services.Configure<SupabaseOptions>(configuration.GetSection(SupabaseOptions.SectionName));
        services.Configure<ClerkOptions>(configuration.GetSection(ClerkOptions.SectionName));

        // LockoutSettings lives in Application (business rule) — bound here by the host
        services.Configure<LockoutSettings>(configuration.GetSection(LockoutSettings.SectionName));

        services.Configure<PasswordPolicyOptions>(configuration.GetSection(PasswordPolicyOptions.SectionName));
        services.Configure<EmailVerificationOptions>(configuration.GetSection(EmailVerificationOptions.SectionName));
        services.Configure<PasswordResetOptions>(configuration.GetSection(PasswordResetOptions.SectionName));

        var supabaseUrl = configuration["Supabase:Url"]?.TrimEnd('/');
        var clerkAuthority = configuration["Clerk:Authority"]?.TrimEnd('/');
        var clerkAudience = configuration["Clerk:Audience"];
        var useClerkAuth = !string.IsNullOrWhiteSpace(clerkAuthority);
        var useSupabaseAuth = !useClerkAuth && !string.IsNullOrWhiteSpace(supabaseUrl);
        var useExternalAuth = useClerkAuth || useSupabaseAuth;

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => useExternalAuth || configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development" ||
                          o.SigningKeys.Any(k => k.IsActive && k.Secret.Length >= 32) ||
                          o.Secret.Length >= 32,
                "A production JWT signing secret of at least 32 characters is required.")
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

        var authentication = services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                if (useClerkAuth)
                {
                    options.Authority = clerkAuthority;
                    if (!string.IsNullOrWhiteSpace(clerkAudience)) options.Audience = clerkAudience;
                    options.RequireHttpsMetadata = true;
                }
                else if (useSupabaseAuth)
                {
                    options.Authority = $"{supabaseUrl}/auth/v1";
                    options.Audience = "authenticated";
                    options.RequireHttpsMetadata = true;
                }
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = useExternalAuth,
                    ValidIssuer = useClerkAuth ? clerkAuthority : useSupabaseAuth ? $"{supabaseUrl}/auth/v1" : jwtOptions.Issuer,
                    ValidateAudience = useClerkAuth ? !string.IsNullOrWhiteSpace(clerkAudience) : true,
                    ValidAudience = useClerkAuth ? clerkAudience : useSupabaseAuth ? "authenticated" : jwtOptions.Audience,
                    ValidateIssuerSigningKey = !useExternalAuth,
                    IssuerSigningKeys = useExternalAuth ? null : signingKeys,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // JTI Blacklist check via JwtBearerEvents (no middleware needed)
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        if (useExternalAuth)
                        {
                            var email = ctx.Principal?.FindFirst("email")?.Value?.Trim().ToLowerInvariant();
                            if (string.IsNullOrWhiteSpace(email))
                            {
                                ctx.Fail("External identity token does not contain an email claim.");
                                return;
                            }

                            var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                            var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email, ctx.HttpContext.RequestAborted);
                            if (user is null)
                            {
                                var studentRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Student", ctx.HttpContext.RequestAborted);
                                if (studentRole is null) { ctx.Fail("Student role is not configured."); return; }
                                var now = DateTime.UtcNow;
                                user = new Platform.Domain.Entities.User
                                {
                                    FullName = email.Split('@')[0], Email = email,
                                    PasswordHash = "supabase-auth-managed",
                                    RoleId = studentRole.Id, Role = studentRole,
                                    IsActive = true, EmailConfirmed = true,
                                    CreatedAt = now, UpdatedAt = now
                                };
                                db.Users.Add(user);
                                await db.SaveChangesAsync(ctx.HttpContext.RequestAborted);
                            }
                            if (!user.IsActive) { ctx.Fail("Account is disabled."); return; }

                            var identity = (ClaimsIdentity)ctx.Principal!.Identity!;
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
                            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role?.Name ?? "Student"));
                            foreach (var permission in PermissionsFor(user.Role?.Name))
                                identity.AddClaim(new Claim("permission", permission));
                            return;
                        }

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
                o.UseNpgsqlConnection(hangfireConnection)));

        services.AddHangfireServer(opts =>
        {
            opts.WorkerCount = 4;
            opts.Queues = ["default", "critical"];
        });

        return services;
    }

    private static string[] PermissionsFor(string? role) => role?.ToLowerInvariant() switch
    {
        "admin" => ["users.manage", "courses.manage", "lessons:write", "exams.manage", "homeworks.manage", "challenges.manage", "announcements.manage", "live-sessions.manage", "plans.manage", "analytics:read", "system:settings:manage"],
        "teacher" => ["courses.manage", "lessons:write", "exams.manage", "homeworks.manage", "challenges.manage", "announcements.manage", "live-sessions.manage", "plans.manage"],
        _ => []
    };
}
