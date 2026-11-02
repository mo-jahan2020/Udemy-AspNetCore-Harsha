using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SabtAhval.Common;
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Middlewares
{
    // سومین میان افزاری که اجرا می شود
    public class IpWhitelistMiddleware
    {
        private readonly RequestDelegate _next;

        public IpWhitelistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IIpWhitelistService ipWhitelistService, IAuditService auditService)
        {
            // آدرس های خاص از این قانون مستثنی هستند
            if (Constants.IsBypassedPath(context.Request.Path))
            {
                await _next(context);
                return;
            }
            // آی پی درخواست کننده گرفته شده
            string? ip = context.Connection.RemoteIpAddress?.ToString();

            // چک سراسری؛ clientId هنوز شناخته نیست (کلید در دروازه بعدی چک می‌شود)
            bool isAllowed = await ipWhitelistService.IsIpAllowedAsync(ip, null);

            if (!isAllowed)
            {
                //  آی پی معتبر نیست
                await auditService.LogRejectedAsync($"IP '{ip}' is not whitelisted. Path: {context.Request.Path}");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "Your IP is not allowed." });
                return; // کوتاه‌کردن پایلاین: درخواست به کلید و کنترلر نمی‌رسد
            }
            //  حال که آی پی معتبر است به مرحله بعد برو
            await _next(context);
        }
    }

    public static class IpWhitelistMiddlewareExtensions
    {
        public static IApplicationBuilder UseIpWhitelistMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<IpWhitelistMiddleware>();
    }
}