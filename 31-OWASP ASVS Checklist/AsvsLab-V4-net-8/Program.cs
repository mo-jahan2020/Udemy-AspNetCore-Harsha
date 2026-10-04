using AsvsLab.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// ⚙️ بند V14.3.3 استاندارد OWASP ASVS 4.0.3:
// «اطلاعات نسخه اجزای سیستم نباید در هدر HTTP یا هر بخش دیگر پاسخ افشا شود»
// با AddServerHeader = false وب‌سرور Kestrel هدر "Server" را ارسال نمی‌کند.
// ============================================================================
builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false; // حذف هدر Server که نسخه وب‌سرور را لو می‌دهد
});

// ============================================================================
// ⚙️ بند V7.4.1 استاندارد OWASP ASVS 4.0.3:
// «هنگام بروز خطای غیرمنتظره، یک پیام عمومی به کاربر نمایش داده شود»
// صفحه خطای عمومی /Error جایگزین نمایش Stack Trace می‌شود.
// ============================================================================
builder.Services.AddControllersWithViews();

// ============================================================================
// ⚙️ بندهای V3.3.2 و V3.4.1 تا V3.4.3 استاندارد OWASP ASVS 4.0.3:
//  - V3.3.2 : نشست باید پس از دوره بی‌فعالیتی مشخص منقضی شود (اینجا: ۱۵ دقیقه)
//  - V3.4.1 : کوکی نشست باید دارای ویژگی Secure باشد
//  - V3.4.2 : کوکی نشست باید دارای ویژگی HttpOnly باشد (جلوگیری از سرقت با JS)
//  - V3.4.3 : کوکی نشست باید دارای ویژگی SameSite باشد (جلوگیری از CSRF)
// ============================================================================
builder.Services.AddSession(options =>
{
    // زمان بی‌فعالیتی مجاز؛ پس از آن نشست به‌صورت خودکار منقضی می‌شود
    options.IdleTimeout = TimeSpan.FromMinutes(15);          // V3.3.2
    options.Cookie.HttpOnly = true;                          // V3.4.2
    options.Cookie.SameSite = SameSiteMode.Lax;              // V3.4.3
    // توجه: در محیط توسعه روی http اجرا می‌کنیم؛ SameAsRequest باعث می‌شود
    // دموی محلی کار کند. در انتشار واقعی (Production) حتماً Secure = Always
    // قرار دهید تا بند V3.4.1 به‌طور کامل محقق شود.
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // V3.4.1
    options.Cookie.Name = "__Host-AsvsLab.Session"; // V3.4.4 - پیشوند __Host-
});

// ============================================================================
// ⚙️ بند V8.3.7 استاندارد OWASP ASVS 4.0.3:
// «داده‌های حساس باید با الگوریتم‌های تأییدشده که محرمانگی و یکپارچگی را
//  همزمان فراهم کنند، رمزنگاری شوند»
// سرویس Data Protection خود ASP.NET Core از AES-256-CBC + HMAC (رمزنگاری
// احرازشده) استفاده می‌کند و کلیدها را به‌صورت خودکار مدیریت می‌کند.
// ============================================================================
builder.Services.AddDataProtection();

// ============================================================================
// ⚙️ سرویس‌های آموزشی پروژه:
//  - PasswordHasher     : هش رمز عبور با PBKDF2 (بندهای V2.4.1 تا V2.4.3)
//  - DemoUserStore      : انبار کاربران آزمایشی (در حافظه - فقط برای آموزش)
//  - RedisLockoutService: قفل‌سازی حساب توزیع‌شده با Redis
//    ⚠️ نکته مهم: برای اجرای درست بخش‌های وابسته به Redis، سرویس Redis باید
//    روی رایانه شما نصب و در حال اجرا باشد (پیش‌فرض: localhost:6379)
// ============================================================================
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<DemoUserStore>();
builder.Services.AddSingleton<RedisLockoutService>();

var app = builder.Build();

// ============================================================================
// ⚙️ بند V7.4.3 استاندارد OWASP ASVS 4.0.3:
// «باید یک "آخرین خط دفاعی" برای مدیریت خطاها وجود داشته باشد که همه
//  استثناهای مدیریت‌نشده را بگیرد»
// UseExceptionHandler همان آخرین خط دفاعی در ASP.NET Core است.
// ============================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // V7.4.1 - پیام عمومی خطا
    app.UseHsts(); // V14.4.5 - هدر Strict-Transport-Security در انتشار واقعی
}
else
{
    app.UseExceptionHandler("/Home/Error"); // برای دموی محلی هم فعال می‌کنیم
}

// ============================================================================
// ⚙️ بند V14.4.x - هدرهای امنیتی پاسخ‌های HTTP
// یک میان‌افزار سفارشی که هدرهای امنیتی را به همه پاسخ‌ها اضافه می‌کند
// (توضیح کامل هر هدر در کنترلر ConfigurationController - دموی هدرهای امنیتی)
// ============================================================================
app.Use(async (context, next) =>
{
    // V14.4.3 - جلوگیری از اجرای اسکریپت‌های غیرمجاز (کاهش اثر XSS)
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:";
    // V14.4.4 - جلوگیری از حدس زدن نوع محتوا (MIME Sniffing)
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    // V14.4.6 - محدود کردن اطلاعات ارسالی در هدر Referer
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    // V14.4.7 - جلوگیری از قرارگرفتن صفحه در iframe سایت‌های دیگر (Clickjacking)
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});

// ----------------------------------------------------------------------------
// ⚠️ استفاده از Bootstrap به‌صورت آفلاین (فایل‌های محلی در wwwroot/lib):
// این کار علاوه بر دسترسی بدون اینترنت، بند V14.2.3 را نیز پوشش می‌دهد؛
// زیرا وقتی فایل‌ها از CDN بیرونی لود می‌شوند باید Subresource Integrity (SRI)
// استفاده شود؛ با فایل محلی این ریسک کاملاً حذف می‌شود.
// ----------------------------------------------------------------------------

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapDefaultControllerRoute();

app.Run();
