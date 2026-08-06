using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Platform.Domain.Exceptions;
using Platform.Domain.Results;
using Error = Platform.Domain.Results.Error;

namespace Platform.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, error) = exception switch
        {
            DomainException domainEx => MapErrorToStatusCode(domainEx.Error),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, Error.Unauthorized("auth.unauthorized", "Unauthorized access.")),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, Error.NotFound("resource.not_found", "Requested resource was not found.")),
            ArgumentException argEx => ((int)HttpStatusCode.BadRequest, Error.Validation("request.invalid", argEx.Message)),
            _ => ((int)HttpStatusCode.InternalServerError, Error.Internal(
                "server.internal_error",
                _env.IsDevelopment() ? exception.Message : "An unexpected server error occurred."))
        };

        context.Response.StatusCode = statusCode;

        var response = Result.Failure(error);
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static (int StatusCode, Error Error) MapErrorToStatusCode(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => (int)HttpStatusCode.BadRequest,
            ErrorType.NotFound => (int)HttpStatusCode.NotFound,
            ErrorType.Conflict => (int)HttpStatusCode.Conflict,
            ErrorType.Forbidden => (int)HttpStatusCode.Forbidden,
            ErrorType.Unauthorized => (int)HttpStatusCode.Unauthorized,
            ErrorType.SubscriptionRequired => (int)HttpStatusCode.PaymentRequired,
            ErrorType.Provider => (int)HttpStatusCode.BadGateway,
            _ => (int)HttpStatusCode.InternalServerError
        };

        return (statusCode, error);
    }
}
