using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
// این برای حالت کامپایل است ، در حالت اجرا این خط یعنی هر جا ICitiesService را دید از CitiesService استفاده کن
// ServiceLifetime ها به این شکل است :

// Transient /per injection: هر بار که سرویس درخواست شود، یک نمونه‌ی کاملاً جدید ساخته می‌شود
// حتی اگر چند بار در همان یک Request درخواست شود.
// خصوصیات آن :
//کوتاه‌ترین طول عمر
//بدون اشتراک state بین درخواست‌کننده‌ها
//مناسب برای سرویس‌های سبک، Stateless (بدون حالت)

// Scoped /per scope (Browser Request) : در طول یک HTTP Request، فقط یک نمونه ساخته می‌شود
// و همان نمونه بین تمام کلاس‌هایی که در همان Request به آن نیاز دارند به اشتراک گذاشته می‌شود.
// با شروع Request جدید، نمونه‌ی جدید ساخته می‌شود.
// خصوصیات آن :
//طول عمر متوسط: محدود به یک Request
//امکان اشتراک state بین کلاس‌های مختلف در یک Request، بدون نگرانی از Thread-Safety بین Requestهای مختلف
//رایج‌ترین Lifetime برای سرویس‌های مرتبط با دیتابیس

//Singleton / for enire Application lifetime:فقط یک بار در کل عمر برنامه از لحظه‌ی اجرای برنامه تا خاموش شدن آن یک نمونه ساخته می‌شود
//و همان یک نمونه برای همه‌ی کاربران و همه‌ی Requestها استفاده می‌شود.
// خصوصیات :
//طولانی‌ترین طول عمر: از شروع تا پایان برنامه
//باید Thread-Safe باشد، چون به‌طور همزمان توسط چندین Request/Thread استفاده می‌شود
//مصرف حافظه کمتر (فقط یک نمونه در کل برنامه)
//مناسب برای داده‌های Global و بدون تغییر مکرر

builder.Services.Add(new ServiceDescriptor(
  typeof(ICitiesService),
  typeof(CitiesService),
  ServiceLifetime.Transient
));
// این هم همان بالایی است
//builder.Services.AddTransient<ICitiesService, CitiesService>();
var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

app.Run();
