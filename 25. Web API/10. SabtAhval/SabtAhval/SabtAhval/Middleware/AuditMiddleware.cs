using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SabtAhval.Common;
using SabtAhval.Models.Entities;
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Middlewares
{
    // اولین میان افزاری که اجرا می شود
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
            //آدرس هایی مانند زیر نبایددر این چرخه قرار گیرند و بلافاصله برای اجرا باید به بعدی بروند
            // /health AND  /swagger /Login
            if (Constants.IsBypassedPath(context.Request.Path))
            {
                await _next(context);
                return;
            }
            //از لحظه ورود درخواست این تایمر فهال شده تا پایان که استاپ میشود
            // تا بتواند زمان اجرای درخواست را در لاگ بیندازد
            var stopwatch = Stopwatch.StartNew();

            // ۱) جریان اصلی پاسخ را نگه می‌داریم و به‌جایش بافر حافظه می‌گذاریم
            var originalResponseBody = context.Response.Body;
            using var responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;

            // ۲) بدنه درخواست فقط یک‌بار خواندنی است؛ EnableBuffering خواندن مجدد
            //    توسط Model Binding کنترلر را ممکن می‌کند
            context.Request.EnableBuffering();
            // ابتدای اجرا این تهی برمیگرداند
            string? requestBody = await ReadRequestBodyAsync(context.Request);

            try
            {
                // سپس چرخه درخواست را ادامه داده اعتبار سنجی ها انجام شده درخواست ارسال و پاسخ دریافت شده
                await _next(context);
                // مقدار دریافت شده در این مقدار ذخیره شده context.Response
            }
            finally
            {
                // پاسخ آمده است پس تایمر خاموش شده
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
            // چرخه ارسال و دریافت به پایان رسیده است
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
        // text[..4000]   // از ابتدا (index 0) تا index 3999 → یعنی 4000 کاراکتر اول
        // text[10..]     // از index 10 تا انتها
        // text[..^5]     // از ابتدا تا 5 کاراکتر مانده به انتها
    }

    public static class AuditMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<AuditMiddleware>();
    }
}