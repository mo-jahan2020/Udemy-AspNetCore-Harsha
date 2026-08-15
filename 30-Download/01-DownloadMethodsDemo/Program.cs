// فایل ورودی برنامه ASP.NET Core
// ساخت شیء Builder برای پیکربندی سرویس‌ها و میان‌افزارها
var builder = WebApplication.CreateBuilder(args);

// افزودن سرویس MVC (کنترلرها + View ها) به کانتینر تزریق وابستگی
builder.Services.AddControllersWithViews();

// افزودن سرویس Razor Pages برای پشتیبانی از صفحات Razor
builder.Services.AddRazorPages();

// افزودن سرویس Server-Side Blazor برای صفحات Blazor
builder.Services.AddServerSideBlazor();

// ساخت اپلیکیشن نهایی از روی Builder
var app = builder.Build();

// فعال‌سازی سرو فایل‌های استاتیک از پوشه wwwroot (برای فایل sample.pdf و ...)
app.UseStaticFiles();

// فعال‌سازی مسیریابی درخواست‌ها
app.UseRouting();

// نگاشت مسیر پیش‌فرض کنترلرها به Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// نگاشت Razor Pages
app.MapRazorPages();

// نگاشت Blazor Hub برای ارتباط SignalR بلیزور
app.MapBlazorHub();

// اجرای برنامه و شروع گوش دادن به درخواست‌ها
app.Run();
