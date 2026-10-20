# پروژه ۲۳ — Configure Services Extension (سازماندهی ثبت سرویس‌ها با Extension Method)

> منبع: پوشه `20. Filters/23. Configure Services Extension`
> آخرین پروژه فصل — پاک‌سازی و آماده‌سازی برای فصل‌های بعدی (Error Handling و Clean Architecture)

---

## 🎯 مبحث این پروژه چیست؟

پس از ۲۲ پروژه، `Program.cs` شلوغ شده است: Serilog، فیلترها، Repositoryها، Serviceها، DbContext، HttpLogging — همه در یک فایل. این پروژه الگوی استانداردی را آموزش می‌دهد که با **Extension Method**، ثبت سرویس‌ها را به کلاس جدا منتقل می‌کند:

```csharp
// Program.cs — تمیز و خلاصه شد:
builder.Services.ConfigureServices(builder.Configuration);
```

و تعریف آن در پوشه جدید `StartupExtensions`:

```text
CRUDExample/
├── Controllers/
├── Filters/
├── Views/
├── StartupExtensions/          ← پوشه جدید
│   └── ConfigureServicesExtension.cs
└── Program.cs
```

## 💡 چرا به این مبحث نیاز داریم؟

1. **خوانایی:** Program.cs باید «فهرست کوتاه مراحل راه‌اندازی» باشد، نه دفترچه ۲۰۰ خطی؛
2. **نگهداری:** افزودن سرویس جدید یعنی ویرایش کلاس Extension، نه گم‌شدن در Program.cs؛
3. **استفاده مجدد:** همین Extension را می‌توانید در پروژه Test یا پروژه دیگری از Solution صدا بزنید؛
4. **مسیر دوره:** در فصل Clean Architecture همین الگو به چند فایل Extension (لایه‌به‌لایه) تبدیل می‌شود — این‌جا پایه‌اش ساخته می‌شود.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/StartupExtensions/ConfigureServicesExtension.cs`

```csharp
using CRUDExample.Filters.ActionFilters;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample
{
 public static class ConfigureServicesExtension
 {
  public static IServiceCollection ConfigureServices(
      this IServiceCollection services, IConfiguration configuration)
  {
   services.AddTransient<ResponseHeaderActionFilter>();   // فیلتر واقعی، برای Factory/Global

   //it adds controllers and views as services
   services.AddControllersWithViews(options => {
    var logger = services.BuildServiceProvider()
        .GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

    options.Filters.Add(new ResponseHeaderActionFilter(logger)
    {
     Key = "My-Key-From-Global",
     Value = "My-Value-From-Global",
     Order = 2
    });
   });

   //add services into IoC container
   services.AddScoped<ICountriesRepository, CountriesRepository>();
   services.AddScoped<IPersonsRepository, PersonsRepository>();

   services.AddScoped<ICountriesService, CountriesService>();
   services.AddScoped<IPersonsService, PersonsService>();

   services.AddDbContext<ApplicationDbContext>(options =>
   {
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
   });

   services.AddTransient<PersonsListActionFilter>();   // برای [ServiceFilter]

   services.AddHttpLogging(options =>
   {
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields
        .RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields
        .ResponsePropertiesAndHeaders;
   });

   return services;   // ← زنجیره‌پذیری (Fluent)
  }
 }
}
```

فایل: `CRUDExample/Program.cs` (بعد از پاک‌سازی)

```csharp
using Serilog;
using CRUDExample;

var builder = WebApplication.CreateBuilder(args);

//Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services,
    LoggerConfiguration loggerConfiguration) => {
 loggerConfiguration
 .ReadFrom.Configuration(context.Configuration)
 .ReadFrom.Services(services);
});

builder.Services.ConfigureServices(builder.Configuration);   // ← همه‌چیز در یک خط

var app = builder.Build();

app.UseSerilogRequestLogging();

if (builder.Environment.IsDevelopment())
 app.UseDeveloperExceptionPage();

app.UseHttpLogging();

if (builder.Environment.IsEnvironment("Test") == false)
 Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseStaticFiles();
app.MapControllers();

app.Run();

public partial class Program { }
```

## ⚙️ آناتومی الگو

| جزء | توضیح |
|---|---|
| کلاس `static` + متد `static` | Extension Method روی `IServiceCollection` |
| `this IServiceCollection services` | اولین پارامتر با `this` یعنی «این متد به IServiceCollection اضافه می‌شود» |
| `IConfiguration configuration` | مقادیر لازم (مثل Connection String) به‌صورت پارامتر می‌آیند، نه از متغیر سراسری |
| `return services;` | امکان نوشتن `services.ConfigureServices(cfg).AddXxx()` |

نکته دیگر: فیلتر سراسری حالا با **Object Initializer** پیکربندی می‌شود (`Key = ..., Value = ..., Order = ...`) — چون `ResponseHeaderActionFilter` در پروژه ۲۰ به کلاسی با پراپرتی‌های settable تبدیل شده بود.

## 🧪 رفتار برنامه در اجرا

هیچ رفتار جدیدی اضافه نشده؛ برنامه دقیقاً مثل قبل کار می‌کند (لاگ‌ها، فیلترها، CRUD). سنجش موفقیت این پروژه، **کیفیت کد** است: Program.cs از حدود ۴۰ خط سرویس‌ثبتی به چند خط رسید و مسئولیت‌ها تفکیک شد.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `this IServiceCollection` | تعریف Extension Method برای DI |
| `IConfiguration` | دسترسی به appsettings داخل Extension |
| `return IServiceCollection` | زنجیره‌پذیری فراخوانی‌ها |
| پوشه `StartupExtensions/` | قرارداد سازماندهی کلاس‌های راه‌اندازی |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **Configuration را پاس بدهید، `builder.Configuration` را داخل کلاس نگیرید:** وابستگی به `WebApplicationBuilder` کلاس Extension را به Program گره می‌زند؛ پارامتر `IConfiguration` تمیزتر و تست‌پذیرتر است.
2. **`BuildServiceProvider` هنوز همان هشدار قبلی را دارد:** خط لاگرِ سراسری در Extension هم anti-pattern است؛ در نسخه‌های تولیدی، فیلتر سراسری را با `options.Filters.AddService<...>()` و ثبت در DI جایگزین کنید (پروژه ۱۸ راه را نشان داد).
3. **یکی یا چند Extension؟** با رشد پروژه، معمولاً به‌جای یک متد بزرگ، چند Extension (AddRepositories، AddServices، AddFilters و...) می‌سازند؛ همین الان ساختار را طوری بچینید که شکستن آن راحت باشد.
4. **namespace را ساده نگه دارید:** کلاس در namespace `CRUDExample` است تا فراخوانی بدون using اضافه انجام شود (`builder.Services.ConfigureServices(...)`).

## ✅ خلاصه

پروژه پایانی فصل، هیچ ویژگی جدیدی به برنامه اضافه نکرد؛ به‌جای آن با یک Extension Method به‌نام `ConfigureServices`، ثبت تمام سرویس‌ها (فیلترها، Repositoryها، Serviceها، DbContext، HttpLogging) را از Program.cs به کلاس `StartupExtensions/ConfigureServicesExtension.cs` منتقل کرد. Program حالا نقشه‌ای خوانا از راه‌اندازی است و فصل Filters با کدی تمیز به پایان می‌رسد — آماده برای فصل Error Handling.
