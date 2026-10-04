// ============================================================================
//  Program.cs — نقطه ورود برنامه آموزشی تزریق وابستگی
// ----------------------------------------------------------------------------
//  در این فایل:
//   1) ثبت سرویس‌های سفارشی با سه Lifetime مختلف:
//        - Transient (موقت): هر بار تزریق، یک نمونه جدید
//        - Scoped   (محدود به درخواست): در طول یک HTTP Request یک نمونه ثابت
//        - Singleton (تک‌نمونه): در کل عمر برنامه فقط یک نمونه
//   2) ثبت سرویس‌های داخلی ASP.NET Core (مثل IWebHostEnvironment, ILogger<T>)
//   3) پیکربندی MVC با Controller و View
// ============================================================================

using DIEduDemo.Data;
using DIEduDemo.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// بخش ۱: ثبت سرویس‌ها در Container تزریق وابستگی
// ----------------------------------------------------------------------------

// ثبت سرویس‌های MVC (Controller + Views). این سرویس‌ها داخلی ASP.NET Core هستند.
// AddControllersWithViews تمام نیازمندی‌های یک پروژه استاندارد MVC را ثبت می‌کند.
builder.Services.AddControllersWithViews();

// ----------------------------------------------------------------------------
// ۱) ثبت سرویس‌های سفارشی با Lifetimeهای مختلف
// ----------------------------------------------------------------------------
// نکته آموزشی: هر سه سرویس زیر از یک اینترفیس مشترک IOperationGuid پیاده‌سازی می‌کنند
// اما با Lifetime متفاوت ثبت می‌شوند تا تفاوت رفتارشان را در خروجی ببینیم.

// AddTransient → هر بار که یک شیء از این سرویس درخواست شود، یک نمونه جدید ساخته می‌شود.
// مناسب برای: سرویس‌های سبک، Stateless، یا زمانی که هر بار باید شیء تازه داشته باشیم.
builder.Services.AddTransient<IOperationTransient, OperationGuidService>();

// AddScoped → در طول یک HTTP Request یک نمونه ثابت برمی‌گرداند؛
// درخواست بعدی، نمونه جدید می‌سازد.
// مناسب برای: DbContext، UnitOfWork، سرویس‌هایی که State درخواست را نگه می‌دارند.
builder.Services.AddScoped<IOperationScoped, OperationGuidService>();

// AddSingleton → در کل عمر برنامه فقط یک نمونه ساخته می‌شود.
// مناسب برای: Cache، Configuration، سرویس‌های گران‌قیمت ساخت شیء.
// هشدار: Singletonها باید Thread-safe باشند.
builder.Services.AddSingleton<IOperationSingleton, OperationGuidService>();

// ----------------------------------------------------------------------------
// ۲) ثبت سرویس‌های آموزشی برای نمایش انواع تزریق
// ----------------------------------------------------------------------------
// ConstructorInjectionDemo: وابستگی‌ها را از طریق Constructor دریافت می‌کند (رایج‌ترین حالت)
builder.Services.AddTransient<ConstructorInjectionDemo, ConstructorInjectionDemo>();

// MethodInjectionDemo: وابستگی به‌صورت پارامتر متد تزریق می‌شود
builder.Services.AddTransient<MethodInjectionDemo, MethodInjectionDemo>();

// PropertyInjectionDemo: تزریق از طریق Property (با الگوی Action/Factory)
// نکته: ASP.NET Core به‌صورت پیش‌فرض Property Injection را پشتیبانی نمی‌کند،
// بنابراین با استفاده از Factory در زمان ثبت سرویس، Property را دستی مقداردهی می‌کنیم.
builder.Services.AddTransient<PropertyInjectionDemo>(sp =>
{
    // ابتدا نمونه‌ای از کلاس می‌سازیم
    var instance = new PropertyInjectionDemo();

    // سپس Propertyها را از Container می‌گیریم و به شیء تزریق می‌کنیم
    instance.SingletonOp = sp.GetRequiredService<IOperationSingleton>();
    instance.Logger = sp.GetRequiredService<ILogger<PropertyInjectionDemo>>();

    return instance;
});

// ----------------------------------------------------------------------------
// ۳) سرویس سفارشی دیگر با اینترفیس (مثال اضافی)
// ----------------------------------------------------------------------------
// IOrderProcessor → منطق پردازش سفارش (نمونه‌ای واقعی‌تر از یک سرویس بیزینسی)
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();

// ILogger<T> یک سرویس داخلی ASP.NET Core است که به‌صورت خودکار ثبت می‌شود
// و می‌توانیم آن را در Constructor هر کلاسی تزریق کنیم.

// ----------------------------------------------------------------------------
// ۴) مثال ترکیبی: سرویسی که خودش به چند سرویس دیگر وابسته است
// ----------------------------------------------------------------------------
// LifetimeInspector: این سرویس هر سه نوع IOperation را در Constructor می‌گیرد
// تا بتوانیم در یک فرم خروجی، تفاوت Lifetimeها را در یک لحظه مقایسه کنیم.
builder.Services.AddScoped<LifetimeInspector>();

// ============================================================================
// بخش ۵: Keyed Services (دات‌نت 8 به بعد)
// ----------------------------------------------------------------------------
//  Keyed Services اجازه می‌دهد چند پیاده‌سازی از یک اینترفیس با کلیدهای مختلف
//  ثبت شوند. سپس با [FromKeyedServices("key")] یا sp.GetKeyedService<T>("key")
//  بازگردانده می‌شوند. این کار الگوی Strategy را آسان می‌کند.
//
//  مثال: سه سرویس اعلان (Email / SMS / Push) همه INotificationService را
//  پیاده‌سازی می‌کنند، اما با کلیدهای مختلف ثبت می‌شوند.
// ============================================================================

// ثبت با AddKeyedScoped — کلیدها رشته‌ای هستند
builder.Services.AddKeyedScoped<INotificationService, EmailNotificationService>("email");
builder.Services.AddKeyedScoped<INotificationService, SmsNotificationService>("sms");
builder.Services.AddKeyedScoped<INotificationService, PushNotificationService>("push");

// نکته: می‌توان از AddKeyedTransient و AddKeyedSingleton هم استفاده کرد.
// مثلاً برای سرویس‌های گران، AddKeyedSingleton مناسب‌تر است.

// ============================================================================
// بخش ۶: DbContext با EF Core In-Memory
// ----------------------------------------------------------------------------
//  نکات آموزشی:
//   - DbContext در ASP.NET Core به‌صورت Scoped ثبت می‌شود (پیش‌فرض).
//   - این یعنی در یک HTTP Request، یک نمونه DbContext به همه‌ی سرویس‌ها داده می‌شود.
//   - In-Memory Database برای آموزش/تست است؛ در Production از SQL Server یا
//     PostgreSQL یا SQLite استفاده کنید.
//
//  نکته فنی: database name در UseInMemoryDatabase باید در سراسر برنامه ثابت باشد
//  تا داده‌ها بین Requestها حفظ شوند. اگر در هر Request یک نام جدید تولید کنیم،
//  دیتابیس جدید خالی می‌سازد و داده‌های قبلی از دست می‌روند.
// ============================================================================

// ثبت DbContext با In-Memory Provider
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("DIEduDemoDb");
    // در محیط Development، لاگ‌گیری SQL اجراشده را فعال می‌کنیم
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});

// ثبت Repository که از DbContext استفاده می‌کند
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// نکته: هنگام استفاده از AddDbContext، خود DbContext به‌صورت Scoped ثبت می‌شود.
// بنابراین IProductRepository هم باید Scoped باشد تا با DbContext سازگار باشد.

var app = builder.Build();

// ----------------------------------------------------------------------------
// Seed دیتابیس In-Memory در زمان راه‌اندازی برنامه
// ----------------------------------------------------------------------------
// نکته: In-Memory Database داده‌ها را در حافظه نگه می‌دارد.
// با هر بار راه‌اندازی برنامه، دیتابیس از نو ساخته می‌شود، اما داده‌های Seed
// (که در OnModelCreating تعریف کرده‌ایم) به‌صورت خودکار اضافه می‌شوند.
// EnsureCreated مطمئن می‌شود که دیتابیس ساخته شده است.
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    try
    {
        var db = sp.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("AppDbContext ساخته شد. تعداد محصولات اولیه: {Count}", db.Products.Count());
    }
    catch (Exception ex)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "خطا در ساخت دیتابیس");
    }
}

// ----------------------------------------------------------------------------
// بخش ۲: پیکربندی Middleware Pipeline
// ----------------------------------------------------------------------------

// در محیط Development، صفحات خطای توسعه‌دهنده را فعال می‌کنیم
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // پیش‌فرض HSTS را در محیط Production فعال می‌کنیم
    app.UseHsts();
}

// هدایت HTTP به HTTPS
app.UseHttpsRedirection();

// ارائه فایل‌های استاتیک (wwwroot)
app.UseStaticFiles();

// مسیریابی (Routing)
app.UseRouting();

// مجوزها (Authorization) — حتی اگر فعلاً نیاز نداریم، به‌ترتیب Pipeline اضافه می‌شود
app.UseAuthorization();

// تعریف Pattern پیش‌فرض مسیرها: /Controller/Action/id?
//app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
//OR
app.MapDefaultControllerRoute();
app.Run();
