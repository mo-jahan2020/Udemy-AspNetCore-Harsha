// =================================================================
// 1. ساخت WebApplicationBuilder
// =================================================================
// CreateBuilder: متد factory برای ساخت یک سازنده‌ی پیش‌فرض برنامه‌ی وب
// این سازنده، تنظیمات پیش‌فرض مثل Logging، Configuration و ... رو فراهم می‌کنه
// (می‌تونستی آرگومان args هم بهش بدی، ولی اختیاریه)
var builder = WebApplication.CreateBuilder();

// =================================================================
// 2. ساخت WebApplication از روی builder
// =================================================================
// Build: سازنده رو به یک WebApplication تبدیل می‌کنه
// در این مرحله DI Container آماده میشه ولی Middleware Pipeline هنوز خالیه
var app = builder.Build();

// =================================================================
// 3. اولین Middleware (با Use)
// =================================================================
// Use: یک Middleware به Pipeline اضافه می‌کنه
// این Middleware می‌تونه قبل و بعد از Middlewareهای بعدی کار انجام بده
// چون await next(context) رو صدا می‌زنه، اجازه میده Middleware بعدی هم اجرا بشه

//// ✅ شکل خلاصه (Type Inference) - کامپایلر خودش می‌فهمه و نیاز به تعریف نوع کانتکس نیست
///app.Use(async (context, next) => { ... });

//// ✅ شکل صریح - خودت می‌نویسی همون کار رو می‌کنه
/////app.Use(async (HttpContext context, RequestDelegate next) => { ... });

app.Use(async (context, next) =>
{
    // نوشتن "Hello " در پاسخ HTTP (هنوز به کلاینت ارسال نشده)
    await context.Response.WriteAsync("Hello ");
    // فراخوانی Middleware بعدی در زنجیره
    // اگه این خط نباشه، زنجیره متوقف میشه و Middlewareهای بعدی اجرا نمیشن
    await next(context);
});

// =================================================================
// 4. دومین Middleware (با Use)
// =================================================================
// این هم Use هست، پس می‌تونه next رو صدا بزنه
// ترتیب اجرا: 1 → 2 → ... → Terminal
app.Use(async (context, next) =>
{
    // نوشتن "World " در پاسخ (بعد از "Hello " از Middleware قبلی)
    await context.Response.WriteAsync("World ");
    // فراخوانی Middleware بعدی (در اینجا Terminal Middleware)
    await next(context);
});

// =================================================================
// 5. Terminal Middleware (با Run)
// =================================================================//
// Run: یک TERMINAL Middleware اضافه می‌کنه (آخرین Middleware در زنجیره)
// نکته‌ی مهم: این app.Run با app.Run() پایینی فرق داره!
//   - app.Run(RequestDelegate) = اضافه کردن Middleware پایانی
//   - app.Run() = شروع به گوش دادن به درخواست‌ها (WebApplication.Run)
//
// این Middleware دیگه next رو صدا نمیزنه، پس Middleware بعدی اجرا نمیشه
// نوع context به صورت صریح (HttpContext) نوشته شده
app.Run(async (HttpContext context) =>
{
    // نوشتن "Ali11" در پاسخ (آخرین بخش خروجی)
    await context.Response.WriteAsync("Ali11");
});

// =================================================================
// 6. شروع برنامه
// =================================================================
// این app.Run() متد WebApplication.Run هست (نه IApplicationBuilder.Run)
// کارش: شروع به گوش دادن به درخواست‌های HTTP و اجرای برنامه
// تا وقتی این خط اجرا نشه، برنامه شروع نمیشه (اما Pipeline ساخته شده)
app.Run();