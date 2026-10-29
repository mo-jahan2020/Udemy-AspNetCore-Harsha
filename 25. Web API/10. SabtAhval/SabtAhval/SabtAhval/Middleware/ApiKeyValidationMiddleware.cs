using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SabtAhval.Common;
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Middlewares
{
    public class ApiKeyValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService, IAuditService auditService)
        {
            if (Constants.IsBypassedPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // 1) هدر کلید وجود دارد؟
            if (!context.Request.Headers.TryGetValue(Constants.ApiKeyHeader, out var apiKey) ||
                string.IsNullOrWhiteSpace(apiKey))
            {
                await auditService.LogRejectedAsync("API key header is missing.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "API key header is missing." });
                return;
            }

            // 2) کلید معتبر است؟ (هش‌کردن داخل سرویس انجام می‌شود)
            var client = await apiKeyService.GetByApiKeyAsync(apiKey!);

            if (client is null)
            {
                // هشدار امنیتی: هرگز مقدار واقعی کلید را در لاگ ننویس!
                await auditService.LogRejectedAsync("Invalid or inactive API key.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid or inactive API key." });
                return;
            }

            // 3) کاربر شناخته شد → در Items بگذار تا کنترلر و Audit استفاده کنند
            context.Items[Constants.ApiClientItemKey] = client;

            await _next(context);
        }
    }

    public static class ApiKeyValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyValidationMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ApiKeyValidationMiddleware>();
    }
}