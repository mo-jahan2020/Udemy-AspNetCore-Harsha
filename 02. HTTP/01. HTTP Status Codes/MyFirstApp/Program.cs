// مرحله ۱: ساخت Builder با تنظیمات پیش‌فرض
// این خط یک WebApplicationBuilder می‌سازد که شامل:
// - Configuration (appsettings.json, environment variables, ...)
// - Logging
// - Dependency Injection Container
var builder = WebApplication.CreateBuilder(args);

// مرحله ۲: ساخت WebApplication از روی Builder
// این خط تمام سرویس‌های ثبت‌شده را جمع‌آوری کرده و یک برنامه آماده می‌سازد
var app = builder.Build();

// مرحله ۳: اضافه کردن یک Terminal Middleware
// این متد یک Middleware به انتهای Pipeline اضافه می‌کند
// نکته مهم: این متد سرور را START نمی‌کند، فقط یک Middleware ثبت می‌کند!
/*
✅ یک Terminal Middleware به Pipeline اضافه می‌کند
✅ برای هر درخواست HTTP اجرا می‌شود
❌ سرور را شروع نمی‌کند
✅ می‌توانید چندین بار آن را فراخوانی کنید (اما فقط آخری اجرا می‌شود)
 * */
app.Run(async (HttpContext context) =>
{
    // این کد برای هر HTTP Request اجرا می‌شود
    // HttpContext شامل تمام اطلاعات Request و Response است

    if (1 == 1)
    {
        // تنظیم Status Code به 200 (OK)
        context.Response.StatusCode = 200;
    }
    else
    {
        // تنظیم Status Code به 400 (Bad Request)
        context.Response.StatusCode = 400;
    }

    // نوشتن پاسخ - این دو خط به هم می‌چسبند
    await context.Response.WriteAsync("Hello");
    await context.Response.WriteAsync(" World");
    // خروجی نهایی: "Hello World"
});

// مرحله ۴: شروع سرور و گوش دادن به درخواست‌ها
// این متد:
// - سرور Kestrel را Start می‌کند
// - شروع به گوش دادن به پورت (معمولاً 5000 یا 5001) می‌کند
// - تا زمانی که برنامه متوقف شود (Ctrl+C)، ادامه می‌یابد (Blocking call)
/*
✅ سرور Kestrel را شروع می‌کند
✅ شروع به گوش دادن به درخواست‌ها می‌کند
✅ یک Blocking call است (یعنی برنامه اینجا متوقف می‌شود و منتظر درخواست‌ها می‌ماند)
❌ هیچ Middleware اضافه نمی‌کند
 */
app.Run();