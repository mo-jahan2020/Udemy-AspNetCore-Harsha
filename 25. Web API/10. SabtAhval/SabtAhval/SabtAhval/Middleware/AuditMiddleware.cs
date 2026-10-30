using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SabtAhval.Common;
using SabtAhval.Models.Entities;
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Middlewares
{
    public class AuditMiddleware
    {
        private const int MaxBodyLength = 4000;   // سقف ذخیره بدنه‌ها تا جدول لاگ متورم نشود

        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            if (Constants.IsBypassedPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            // ۱) جریان اصلی پاسخ را نگه می‌داریم و به‌جایش بافر حافظه می‌گذاریم
            var originalResponseBody = context.Response.Body;
            using var responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;

            // ۲) بدنه درخواست فقط یک‌بار خواندنی است؛ EnableBuffering خواندن مجدد
            //    توسط Model Binding کنترلر را ممکن می‌کند
            context.Request.EnableBuffering();
            string? requestBody = await ReadRequestBodyAsync(context.Request);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // ۳) خواندن پاسخِ نوشته‌شده در بافر
                string? responseBody = await ReadResponseBodyAsync(context.Response);

                // ۴) برگرداندن جریان اصلی و کپی پاسخ — تا کلاینت چیزی از دست ندهد
                context.Response.Body = originalResponseBody;
                responseBuffer.Position = 0;
                await responseBuffer.CopyToAsync(originalResponseBody);

                // ۵) ساخت رکورد با همه اطلاعات واقعی
                var client = context.Items[Constants.ApiClientItemKey] as ApiClient;

                var log = new ApiRequestLog
                {
                    ApiClientId = client?.Id,                  // ردشده‌ها → null
                    Username = client?.Username,
                    ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                    HttpMethod = context.Request.Method,
                    RequestPath = Truncate(context.Request.Path.Value),
                    QueryString = Truncate(context.Request.QueryString.Value),
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    StatusCode = (short)context.Response.StatusCode,
                    DurationMs = (int)stopwatch.ElapsedMilliseconds
                };

                // ۶) ثبت در دیتابیس — خطای داخلی آن به بیرون نشت نمی‌کند
                await auditService.LogRequestAsync(log);
            }
        }

        private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength is null || request.ContentLength == 0)
                return null;

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);

            string body = await reader.ReadToEndAsync();

            // مهم: اشاره‌گر را به اول برگردان تا Model Binding کنترلر هم بخواند
            request.Body.Position = 0;

            return Truncate(body);
        }

        private static async Task<string?> ReadResponseBodyAsync(HttpResponse response)
        {
            if (response.Body.Length == 0)
                return null;

            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);

            string body = await reader.ReadToEndAsync();
            return Truncate(body);
        }

        private static string? Truncate(string? text)
            => string.IsNullOrEmpty(text) ? null
             : text.Length <= MaxBodyLength ? text
             : text[..MaxBodyLength] + " ...[truncated]";
    }

    public static class AuditMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<AuditMiddleware>();
    }
}