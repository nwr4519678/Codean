using Microsoft.AspNetCore.Http;

namespace Platform.Api.Middleware;

/// <summary>
/// Injects enterprise-grade security response headers on every response.
/// Covers HSTS, CSP, X-Content-Type-Options, X-Frame-Options, Referrer-Policy, and Permissions-Policy.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME-type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // XSS protection (legacy browsers)
        headers["X-XSS-Protection"] = "1; mode=block";

        // Control referrer in cross-origin requests
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // HSTS: enforce HTTPS for 1 year, include subdomains
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // Limit browser features (camera, microphone, geolocation, etc.)
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // Content Security Policy — restrictive default
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'";

        await _next(context);
    }
}
