using System.Diagnostics;

namespace MovieInformationProvider.Middleware;

/// <summary>
/// Logs HTTP requests and responses with timing information
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string method = context.Request.Method;
        string path = context.Request.Path;
        string queryString = context.Request.QueryString.ToString();

        _logger.LogInformation(
            "HTTP {Method} {Path}{QueryString} started",
            method, path, queryString);

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            int statusCode = context.Response.StatusCode;
            LogLevel logLevel = statusCode >= 500
                ? LogLevel.Error
                : statusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(
                logLevel,
                "HTTP {Method} {Path}{QueryString} completed with {StatusCode} in {ElapsedMs}ms",
                method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
