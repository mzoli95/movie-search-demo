using System.Net;
using System.Text.Json;
using MovieInformationProvider.Application.Common.Exceptions;
using MovieInformationProvider.Domain.Common;

namespace MovieInformationProvider.Middleware;

/// <summary>
/// Global exception handler middleware - catches all unhandled exceptions and returns structured error responses
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maps exceptions to HTTP status codes and creates structured error response
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        HttpStatusCode statusCode = exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        ErrorResponse errorResponse = new ErrorResponse
        {
            Message = exception.Message,
            StatusCode = (int)statusCode,
            TraceId = context.TraceIdentifier
        };

        if (exception is ValidationException validationException)
        {
            errorResponse.ValidationErrors = validationException.Errors;
        }

        if (_environment.IsDevelopment())
        {
            errorResponse.StackTrace = exception.StackTrace;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        string json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
