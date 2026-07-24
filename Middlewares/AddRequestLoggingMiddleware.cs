using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class RequestLoggingMiddleware
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
        // Generate a short correlation id (example)
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // Stamp header before calling next middleware
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // Measure elapsed time
        var sw = Stopwatch.StartNew();

        // Entry log line
        var method = context.Request.Method;
        var path = context.Request.Path;
        _logger.LogInformation(
            "Request start: {Method} {Path} CorrelationId={CorrelationId}",
            method, path, correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            // Exit log line (same correlation id)
            _logger.LogInformation(
                "Request finish: {Method} {Path} StatusCode={StatusCode} ElapsedMs={ElapsedMs} CorrelationId={CorrelationId}",
                method,
                path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
    }
}