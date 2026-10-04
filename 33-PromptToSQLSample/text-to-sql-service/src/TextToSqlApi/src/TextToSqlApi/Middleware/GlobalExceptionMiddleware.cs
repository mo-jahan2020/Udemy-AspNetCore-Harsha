using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TextToSqlApi.Models.DTOs;

namespace TextToSqlApi.Middleware;

/// <summary>
/// Middleware برای مدیریت گلوبال استثناها و برگرداندن پاسخ خطای استاندارد ProblemDetails.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    /// <summary>سازنده با تزریق وابستگی‌ها.</summary>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    /// <summary>پردازش درخواست و catch استثناها.</summary>
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

    /// <summary>تبدیل استثنا به پاسخ JSON استاندارد.</summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;
        var (status, title) = ex switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            ArgumentException or InvalidOperationException => (HttpStatusCode.BadRequest, "Bad request"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            TimeoutException => (HttpStatusCode.GatewayTimeout, "Request timed out"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        var errorResponse = new ErrorResponse
        {
            Type = $"https://httpstatuses.io/{(int)status}",
            Title = title,
            Status = (int)status,
            Detail = _env.IsDevelopment() ? ex.ToString() : ex.Message,
            Instance = context.Request.Path,
            TraceId = traceId
        };

        _logger.LogError(ex, "Global exception caught. TraceId={TraceId} Path={Path}", traceId, context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(json);
    }
}
