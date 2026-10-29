using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SabtAhval.Models.Dtos;

namespace SabtAhval.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // اجازه عبور؛ فقط مراقب خطا هستیم
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // کلاینت خودش قطع شده؛ پاسخی برای دادن نیست
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    // دیر شده! هدرها رفته‌اند؛ فقط دوباره پرتاب کن
                    _logger.LogError(ex, "Response already started, cannot return clean error.");
                    throw;
                }

                // نوع خطا → کد وضعیت مناسب
                (int statusCode, string message) = ex switch
                {
                    HttpRequestException => (StatusCodes.Status502BadGateway, "Target API is unreachable."),
                    TaskCanceledException => (StatusCodes.Status504GatewayTimeout, "Target API did not respond in time."),
                    _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
                };

                _logger.LogError(ex, "Unhandled exception for {Path} -> {StatusCode}", context.Request.Path, statusCode);

                context.Response.Clear();
                context.Response.StatusCode = statusCode;

                await context.Response.WriteAsJsonAsync(new ErrorResponse
                {
                    StatusCode = statusCode,
                    Message = message,               // جزئیات داخلی هرگز به کاربر نرو!
                    TraceId = context.TraceIdentifier
                });
            }
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}