// مرحله ۱: ساخت Builder با تنظیمات پیش‌فرض
var builder = WebApplication.CreateBuilder(args);

// مرحله ۲: ساخت WebApplication
var app = builder.Build();

// مرحله ۳: اضافه کردن Terminal Middleware
// این کد برای هر درخواست HTTP اجرا می‌شود
app.Run(async (HttpContext context) =>
{
    // ═══════════════════════════════════════════════════════════
    // بخش اول: کار با Response Headers
    // ═══════════════════════════════════════════════════════════

    // Header یک متادیتا (داده درباره داده) است که همراه Response ارسال می‌شود
    // مرورگر یا کلاینت از این اطلاعات برای پردازش پاسخ استفاده می‌کند

    // ۱. اضافه کردن یک Header سفارشی
    // این Header هیچ معنای خاصی برای مرورگر ندارد
    // اما می‌تواند برای APIها یا دیباگ کردن مفید باشد
    context.Response.Headers["MyKey"] = "my value";

    // ۲. تغییر Header سرور
    // به صورت پیش‌فرض، ASP.NET Core نام سرور را "Kestrel" تنظیم می‌کند
    // با این خط، نام سرور را تغییر می‌دهیم (برای مخفی کردن تکنولوژی استفاده شده)
    context.Response.Headers["Server"] = "My server";

    // ۳. تنظیم Content-Type (بسیار مهم!)
    // این Header به مرورگر می‌گوید که محتوای پاسخ چه نوعی است
    // "text/html" یعنی مرورگر باید محتوا را به عنوان HTML رندر کند
    context.Response.Headers["Content-Type"] = "text/html";

    // دقیقا این مقدار چاپ میشود
    //<h1>Hello</h1><h2>World</h2>
    //context.Response.Headers["Content-Type"] = "text/plain";

    // ═══════════════════════════════════════════════════════════
    // بخش دوم: نوشتن محتوای Response
    // ═══════════════════════════════════════════════════════════

    // نوشتن تگ HTML <h1> (عنوان اصلی)
    // چون Content-Type را text/html تنظیم کردیم، مرورگر این را به عنوان HTML تفسیر می‌کند
    await context.Response.WriteAsync("<h1>Hello</h1>");

    // نوشتن تگ HTML <h2> (عنوان فرعی)
    // این محتوا به ادامه محتوای قبلی اضافه می‌شود
    await context.Response.WriteAsync("<h2>World</h2>");

    // خروجی نهایی در مرورگر:
    // Hello (با فونت بزرگ - h1)
    // World (با فونت متوسط - h2)
});

// مرحله ۴: شروع سرور و گوش دادن به درخواست‌ها
app.Run();