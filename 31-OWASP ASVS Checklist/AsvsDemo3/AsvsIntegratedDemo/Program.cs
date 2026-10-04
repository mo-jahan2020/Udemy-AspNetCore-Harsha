using Microsoft.EntityFrameworkCore;
using Serilog;
using AsvsIntegratedDemo.Models;
using AsvsIntegratedDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// بند OWASP ASVS: V7.1.2 - پیکربندی لاگ‌گذاری امن
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Filter.ByExcluding(MaskingEnricher.MaskSensitiveData)
    .CreateLogger();
builder.Host.UseSerilog();

// بند OWASP ASVS: V4.1.1 - دیتابیس برای کنترل دسترسی
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AsvsMockDb"));

// بند OWASP ASVS: V3.2.1 و V11.1.1 - حافظه کش
builder.Services.AddMemoryCache();

// بند OWASP ASVS: V6.1.1 - سیستم رمزنگاری
builder.Services.AddDataProtection();
builder.Services.AddScoped<ISecureStorageService, SecureStorageService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// بند OWASP ASVS: V14.1.1 - هدرهای امنیتی HTTP
app.Use(async (context, next) => {
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline';");
    await next();
});

// بند OWASP ASVS: V11.1.1 - Middleware محدودیت نرخ درخواست
app.UseMiddleware<RateLimitingMiddleware>();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// مقداردهی اولیه دیتابیس
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Users.Any()) {
        db.Users.Add(new User { Username = "admin", PasswordHash = "hashed_pwd", Role = "Admin" });
        db.Users.Add(new User { Username = "user", PasswordHash = "hashed_pwd", Role = "User" });
        db.SaveChanges();
    }
}

app.Run();
