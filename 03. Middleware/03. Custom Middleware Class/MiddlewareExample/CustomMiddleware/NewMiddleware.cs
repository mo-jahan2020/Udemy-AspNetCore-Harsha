using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MiddlewareExample.Midelwares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    //اگر پروژه شما نوع زیر را دارد نیاز به نصب این پکیج ندارید
    //<Project Sdk="Microsoft.NET.Sdk.Web">
    //اگر این بود باید نصب کنید
    //<Project Sdk="Microsoft.NET.Sdk">
    public class NewMiddleware
    {
        private readonly RequestDelegate _next;

        public NewMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await httpContext.Response.WriteAsync("new midelware started!!\n");
            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class NewMiddlewareExtensions
    {
        public static IApplicationBuilder UseNewMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NewMiddleware>();
        }
    }
}
