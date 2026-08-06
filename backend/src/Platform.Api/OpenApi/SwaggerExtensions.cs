using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Platform.Api.OpenApi;

public static class SwaggerExtensions
{
    public static IServiceCollection AddPlatformOpenApi(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Platform API",
                Version = "v1",
                Description = "Platform Backend REST API",
                Contact = new OpenApiContact
                {
                    Name = "Platform Support",
                    Email = "support@platform.app"
                }
            });

            options.EnableAnnotations();

            // Configure Bearer Authentication in Swagger UI
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' followed by your JWT token (e.g. 'eyJhbGci...')"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UsePlatformOpenApi(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Platform API v1");
            c.RoutePrefix = "swagger";
            c.DocExpansion(DocExpansion.None);
            c.DisplayRequestDuration();
        });

        return app;
    }
}
