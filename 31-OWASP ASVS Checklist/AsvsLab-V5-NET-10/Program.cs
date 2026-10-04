using AsvsLab.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

// ═══════════════════════════════════════════════════════════════════════════
//  AsvsLab — آزمایشگاه آموزشی OWASP ASVS 5.0 (نسخه ۲)
//  نقطه شروع برنامه؛ هر بلوک با کامنت فارسی و شماره بند مربوطه از ASVS 5.0 مشخص شده
// ═══════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// ─── بند V13.4.2 — غیرفعال بودن حالت دیباگ و جلوگیری از نشت اطلاعات سرور ───
// حذف هدر «Server: Kestrel» تا مهاجم نتواند نسخه وب‌سرور را انگشت‌نگاری کند
// (در نسخه 4.0.3 این بند V14.3.3 بود؛ در 5.0 مفهوم آن در فصل پیکربندی آمده است)
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddControllersWithViews();

// ─── بند V6.3.1 / V7.2.4 — احراز هویت واقعی با کوکی احراز هویت ───
// صفحه ورود/ثبت‌نام واقعی با نقش‌ها (Admin/User) و قفل‌گذاری پس از تلاش ناموفق
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);   // طول عمر کوکی احراز هویت
        options.SlidingExpiration = true;
        options.Cookie.Name = "AsvsLab.Auth";
        options.Cookie.HttpOnly = true;                      // بند V3.3.4 — عدم دسترسی جاوااسکریپت
        options.Cookie.SameSite = SameSiteMode.Lax;          // بند V3.3.2 — محافظت در برابر CSRF
        // بند V3.3.1 — روی سرور واقعی (HTTPS) حتماً این خط را فعال کنید:
        // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        // بند V3.3.3 — روی HTTPS بهتر است نام کوکی را با پیشوند __Host- تعریف کنید
    });

// ─── بند V7.1.1 / V7.3.1 — سشن با زمان‌بندی وقفه ۱۵ دقیقه‌ای ───
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);      // وقفه بی‌فعالیتی سشن
    options.Cookie.HttpOnly = true;                      // بند V3.3.4
    options.Cookie.Name = "AsvsLab.Session";
    options.Cookie.SameSite = SameSiteMode.Strict;       // بند V3.3.2
});

// ─── بند V14.2.4 — سرویس Data Protection برای رمزنگاری داده در حالت سکون ───
builder.Services.AddDataProtection();

builder.Services.AddHttpContextAccessor();

// سرویس‌های دمو (توضیح هر کدام داخل خود فایل سرویس آمده است)
builder.Services.AddSingleton<AsvsRefService>();        // متن رسمی بندهای ASVS 5.0 از Data/asvs5.json
builder.Services.AddSingleton<PasswordHasher>();        // PBKDF2 (بند V11.4.2) + سیاست گذرواژه (V6.2.x)
builder.Services.AddSingleton<DemoUserStore>();         // کاربران دمو + فاکتورها (برای دموی IDOR در V8)
builder.Services.AddSingleton<RedisLockoutService>();   // قفل‌گذاری توزیع‌شده — نیازمند سرویس Redis
builder.Services.AddSingleton<InMemoryLockoutService>();// قفل‌گذاری درون‌حافظه‌ای — بدون وابستگی
builder.Services.AddScoped<ProgressService>();          // پیشرفت مطالعه کاربر (ذخیره در سشن)

var app = builder.Build();

// ─── بند V16.5.1 — صفحه خطای عمومی؛ هرگز جزئیات داخلی خطا به کاربر نشان داده نمی‌شود ───
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}");

// ─── بندهای V3.4.1 / V3.4.3 — هدرهای امنیتی روی همه پاسخ‌ها ───
app.Use(async (context, next) =>
{
    var h = context.Response.Headers;
    // بند V3.4.3 — سیاست امنیت محتوا: بارگذاری فقط منابع خودِ سایت
    // نکته آموزشی: در محیط عملیاتی بهتر است style-src هم بدون 'unsafe-inline' باشد؛
    // اینجا برای راحتی نمایش نوارهای پیشرفت بوت‌استرپ مجاز شده است.
    h["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; font-src 'self'; frame-ancestors 'none'; form-action 'self'; base-uri 'self'";
    h["X-Content-Type-Options"] = "nosniff";   // جلوگیری از MIME sniffing
    h["Referrer-Policy"] = "no-referrer";      // عدم افشای آدرس مبدأ
    h["X-Frame-Options"] = "DENY";             // محافظت در برابر clickjacking
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseSession();          // باید قبل از احراز هویت باشد (پیشرفت مطالعه در سشن ذخیره می‌شود)
app.UseAuthentication();   // بند V6 — فعال‌سازی احراز هویت کوکی
app.UseAuthorization();    // بند V8 — فعال‌سازی کنترل دسترسی

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
