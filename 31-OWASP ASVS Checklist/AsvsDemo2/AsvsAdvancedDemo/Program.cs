using Microsoft.EntityFrameworkCore;
using Serilog;
using AsvsAdvancedDemo.Models;
using AsvsAdvancedDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// بند OWASP ASVS: V7.1.2 - لاگ‌گذاری امن و ساختاریافته
// توضیح: در محیط واقعی، لاگ‌ها به سیستم‌های خارجی مثل ELK Stack، Splunk یا Datadog ارسال می‌شوند.
// در این دمو: از Serilog استفاده می‌کنیم و یک فیلتر سفارشی برای ماسک کردن اطلاعات حساس (مثل رمز عبور یا شماره کارت) اضافه می‌کنیم.
// ====================================================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: ""[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"")
    .Filter.ByExcluding(MaskingEnricher.MaskSensitiveData) // فیلتر ماسک‌کردن اطلاعات حساس
    .CreateLogger();
builder.Host.UseSerilog();

// افزودن سرویس‌ها
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery();

// ====================================================================
// بند OWASP ASVS: V4.1.1 - کنترل دسترسی (Authorization)
// توضیح: در محیط واقعی، اطلاعات کاربران و نقش‌ها در یک دیتابیس خارجی (مثل SQL Server یا PostgreSQL) ذخیره می‌شود.
// در این دمو: از EF Core In-Memory Provider برای شبیه‌سازی دیتابیس استفاده می‌کنیم تا کد بدون نصب SQL Server اجرا شود.
// ====================================================================
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AsvsMockDb"));

// ====================================================================
// بند OWASP ASVS: V3.2.1 - مدیریت نشست (Session Management)
// توضیح: در محیط واقعی، Sessionها در یک حافظه توزیع‌شده خارجی مثل Redis یا SQL Server Session State ذخیره می‌شوند.
// در این دمو: از IMemoryCache داخلی دات‌نت برای شبیه‌سازی ذخیره‌سازی سمت سرور استفاده می‌کنیم.
// ====================================================================
builder.Services.AddMemoryCache();

// ====================================================================
// بند OWASP ASVS: V6.1.1 - رمزنگاری داده‌های حساس (Cryptography)
// توضیح: در محیط واقعی، کلیدهای رمزنگاری در یک سیستم مدیریت کلید خارجی (KMS) مثل Azure Key Vault یا HashiCorp Vault نگهداری می‌شوند.
// در این دمو: از IDataProtectionProvider داخلی دات‌نت استفاده می‌کنیم که به صورت خودکار کلیدها را مدیریت و داده‌ها را رمزنگاری می‌کند.
// ====================================================================
builder.Services.AddDataProtection();

// ثبت سرویس‌های سفارشی
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISecureStorageService, SecureStorageService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler(""/Home/Error"");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ====================================================================
// بند OWASP ASVS: V11.1.1 - کنترل منطق کسب‌وکار (Rate Limiting)
// توضیح: در محیط واقعی، این کار توسط یک Web Application Firewall (WAF) خارجی یا Middleware مبتنی بر Redis انجام می‌شود.
// در این دمو: یک Middleware سفارشی نوشتیم که تعداد درخواست‌ها از یک IP را محدود می‌کند.
// ====================================================================
app.UseMiddleware<RateLimitingMiddleware>();

app.MapControllerRoute(name: "default", pattern: ""{controller=Home}/{action=Index}/{id?}"");

// مقداردهی اولیه دیتابیس شبیه‌سازی شده
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Users.Any()) {
        db.Users.Add(new User { Username = ""admin"", PasswordHash = ""hashed_pwd"", Role = ""Admin"" });
        db.Users.Add(new User { Username = ""user"", PasswordHash = ""hashed_pwd"", Role = ""User"" });
        db.SaveChanges();
    }
}

app.Run();
