using Microsoft.Extensions.Caching.Memory;

namespace AsvsAdvancedDemo.Services {
    // بند OWASP ASVS: V11.1.1 - محافظت در برابر اتوماسیون و حملات Brute-Force
    // توضیح: در محیط واقعی، این شمارنده در یک حافظه توزیع‌شده خارجی مثل Redis ذخیره می‌شود تا در محیط‌های Multi-Server کار کند.
    // در این دمو: از IMemoryCache برای شبیه‌سازی این رفتار استفاده می‌کنیم.
    public class RateLimitingMiddleware {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache) {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context) {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? ""unknown"";
            var cacheKey = $""rate_limit_{ip}"";

            if (_cache.TryGetValue(cacheKey, out int requestCount)) {
                if (requestCount >= 5) { // محدودیت: 5 درخواست در بازه زمانی
                    context.Response.StatusCode = 429; // Too Many Requests
                    await context.Response.WriteAsync(""خطا: تعداد درخواست‌های شما بیش از حد مجاز است (V11.1.1)."");
                    return;
                }
                _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(1));
            } else {
                _cache.Set(cacheKey, 1, TimeSpan.FromMinutes(1));
            }

            await _next(context);
        }
    }
}
