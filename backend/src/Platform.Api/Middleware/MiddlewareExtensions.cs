using Microsoft.AspNetCore.Builder;

namespace Platform.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UsePlatformMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<CorrelationMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}
