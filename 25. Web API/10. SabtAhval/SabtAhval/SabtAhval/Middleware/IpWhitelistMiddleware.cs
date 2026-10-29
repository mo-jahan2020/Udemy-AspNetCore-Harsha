using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SabtAhval.Common;
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Middlewares
{
    public class IpWhitelistMiddleware
    {
        private readonly RequestDelegate _next;

        public IpWhitelistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IIpWhitelistService ipWhitelistService, IAuditService auditService)
        {
            if (Constants.IsBypassedPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            string? ip = context.Connection.RemoteIpAddress?.ToString();

            // چک سراسری؛ clientId هنوز شناخته نیست (کلید در دروازه بعدی چک می‌شود)
            bool isAllowed = await ipWhitelistService.IsIpAllowedAsync(ip, null);

            if (!isAllowed)
            {
                await auditService.LogRejectedAsync($"IP '{ip}' is not whitelisted. Path: {context.Request.Path}");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "Your IP is not allowed." });
                return; // کوتاه‌کردن پایلاین: درخواست به کلید و کنترلر نمی‌رسد
            }

            await _next(context);
        }
    }

    public static class IpWhitelistMiddlewareExtensions
    {
        public static IApplicationBuilder UseIpWhitelistMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<IpWhitelistMiddleware>();
    }
}