// تعریف namespace مربوط به Middleware سفارشی

namespace MiddlewareExample.CustomMiddleware
{
    // تعریف کلاس Middleware سفارشی
    // کلاس باید اینترفیس IMiddleware رو پیاده‌سازی کنه
    // این اینترفیس فقط یک متد داره: InvokeAsync
    public class MyCustomMiddleware : IMiddleware
    {
        // متد InvokeAsync: منطق اصلی Middleware
        // این متد برای هر درخواست HTTP فراخوانی میشه
        // پارامترها:
        //   context: اطلاعات درخواست و پاسخ HTTP
        //   next: delegate مربوط به Middleware بعدی در Pipeline
        // نوع بازگشتی: Task (چون عملیات ناهمزمان/async هست)
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // -------------------- مرحله‌ی ۱: قبل از next --------------------
            // این کد قبل از Middlewareهای بعدی اجرا میشه
            // معمولاً برای:
            //   - لاگ کردن شروع درخواست
            //   - بررسی‌های امنیتی (Authentication)
            //   - اندازه‌گیری زمان شروع
            //   - تغییر درخواست
            await context.Response.WriteAsync("My Custom Middleware - Starts\n");
            // -------------------- مرحله‌ی ۲: فراخوانی next --------------------
            // await next(context): درخواست رو به Middleware بعدی در Pipeline می‌سپاره
            // اگه Middleware بعدی وجود نداشته باشه (این آخری باشه)، خطا میده
            // معمولاً برای:
            //   - اجازه دادن به بقیه‌ی Pipeline
            //   - بعد از اتمام، کنترل دوباره به اینجا برمی‌گرده
            await next(context);
            // -------------------- مرحله‌ی ۳: بعد از next --------------------
            // این کد بعد از Middlewareهای بعدی اجرا میشه وقتی Pipeline در حال برگشت هست
            // معمولاً برای:
            //   - لاگ کردن پایان درخواست
            //   - اندازه‌گیری زمان کل
            //   - تغییر پاسخ (Response)
            //   - پاکسازی منابع (Cleanup)
            await context.Response.WriteAsync("My Custom Middleware - Ends\n");
        }
    }
}