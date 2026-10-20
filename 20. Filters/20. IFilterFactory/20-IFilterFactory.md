# پروژه ۲۰ — IFilterFactory (کارخانه ساخت فیلتر)

> منبع: پوشه `20. Filters/20. IFilterFactory`
> پیش‌نیاز: پروژه‌های ۱۸ و ۱۹ (ServiceFilter و Filter Attribute) — این پروژه نقطه اوج فصل است

---

## 🎯 مبحث این پروژه چیست؟

پروژه ۱۹ دو دنیا را مقابل هم گذاشت: سینتکس خوشگل Attribute اما بدون DI؛ یا DI کامل اما با سینتکس TypeFilter/ServiceFilter. **`IFilterFactory`** پل این دو دنیاست:

- یک کلاس Attribute می‌سازیم که `IFilterFactory` را پیاده کند → سینتکس `[ResponseHeaderFilterFactory("k","v",1)]`؛
- متد `CreateInstance(IServiceProvider)` آن، فیلترِ واقعی را **از DI** می‌گیرد و پیکربندی را رویش ست می‌کند → DI هم داریم!

جریان معرفی‌شده در کامنت کد پروژه: **Controller → FilterFactory → Filter**

## 💡 چرا به این مبحث نیاز داریم؟

این الگو استاندارد طلایی ساخت فیلترهای «قابل پیکربندی با آرگومان + تزریق سرویس» است. حتی اتریبیوت‌های داخلی خود ASP.NET Core (مثل `[ServiceFilter]` و `[TypeFilter]`) دقیقاً همین‌طور — با پیاده‌سازی `IFilterFactory` — ساخته شده‌اند. با این الگو:

- سازنده فیلتر واقعی، `ILogger` و هر سرویس دیگری را از DI می‌گیرد (مشکل پروژه ۱۹ حل شد)؛
- در محل استفاده، سینتکس ساده Attribute با آرگومان‌های ثابت داریم؛
- `IsReusable` کنترل می‌کند نمونه ساخته‌شده بین Requestها به اشتراک گذاشته شود یا نه.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/ResponseHeaderActionFilter.cs` (نسخه نهایی — دو کلاس در یک فایل)

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
 // ۱) کلاس Attribute: ورودی کاربر را نگه می‌دارد و فیلتر واقعی می‌سازد
 public class ResponseHeaderFilterFactoryAttribute : Attribute, IFilterFactory
 {
  public bool IsReusable => false;    // نمونه برای هر Request تازه ساخته شود

  private string? Key { get; set; }
  private string? Value { get; set; }
  private int Order { get; set; }

  public ResponseHeaderFilterFactoryAttribute(string key, string value, int order)
  {
   Key = key;
   Value = value;
   Order = order;
  }

  //Controller -> FilterFactory -> Filter
  public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
  {
   // فیلتر واقعی از DI گرفته می‌شود (ILogger و...)
   var filter = serviceProvider.GetRequiredService<ResponseHeaderActionFilter>();

   // پیکربندیِ گرفته‌شده از Attribute اعمال می‌شود
   filter.Key = Key;
   filter.Value = Value;
   filter.Order = Order;

   return filter;
  }
 }

 // ۲) فیلتر واقعی: ساده و وابسته به DI
 public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
 {
  public string Key { get; set; }
  public string Value { get; set; }
  public int Order { get; set; }
  private readonly ILogger<ResponseHeaderActionFilter> _logger;

  public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger)
  {
   _logger = logger;
  }

  public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                           ActionExecutionDelegate next)
  {
   _logger.LogInformation("Before logic - ResponseHeaderActionFilter");

   await next(); // فیلتر بعدی یا Action Method

   _logger.LogInformation("After logic - ResponseHeaderActionFilter");

   context.HttpContext.Response.Headers[Key] = Value;
  }
 }
}
```

**ثبت فیلتر واقعی در DI (در نسخه نهایی این پروژه در StartupExtensions انجام می‌شود):**

```csharp
builder.Services.AddTransient<ResponseHeaderActionFilter>();
```

**استفاده — سینتکس تمیز Attribute:**

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[controller]")]
[ResponseHeaderFilterFactory("My-Key-From-Controller", "My-Value-From-Controller", 3)]
public class PersonsController : Controller { ... }

[ResponseHeaderFilterFactory("MyKey-FromAction", "MyValue-From-Action", 1)]
public async Task<IActionResult> Index(...) { ... }
```

## ⚙️ جریان اجرا

```text
Request
 → ASP.NET Core به Attribute برخورد می‌کند (ResponseHeaderFilterFactoryAttribute)
 → چون IFilterFactory است: CreateInstance(serviceProvider) صدا زده می‌شود
 → فیلتر واقعی از DI گرفته می‌شود (با ILogger تزریق‌شده)
 → Key/Value/Order از روی آرگومان‌های Attribute ست می‌شوند
 → فیلتر واقعی در Pipeline اجرا می‌شود (Before / Action / After / Header)
```

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IFilterFactory` | اینترفیس کارخانه؛ فقط `CreateInstance` و `IsReusable` |
| `CreateInstance(IServiceProvider)` | ساخت/واکشی فیلتر واقعی با دسترسی به DI |
| `IsReusable => false` | false = هر Request نمونه جدید (امن‌تر)؛ true = نمونه مشترک |
| `GetRequiredService<T>()` | واکشی فیلتر واقعی ثبت‌شده در DI |
| `Attribute` | ارث‌بری برای استفاده مستقیم به‌صورت `[...]` |

## 🧪 رفتار برنامه در اجرا

- Header های هر Action مطابق پیکربندی Factory ست می‌شوند؛
- لاگ Before/After حالا از ILogger تزریق‌شده چاپ می‌شود (چیزی که در پروژه ۱۹ حذف شده بود، برگشت!)؛
- اگر `ResponseHeaderActionFilter` را در DI ثبت نکنید، `CreateInstance` خطای "No service for type..." می‌دهد.

## ⚠️ نکات کلیدی و خطاهای رایج

1. **`IsReusable` را با احتیاط روشن کنید:** true یعنی یک نمونه فیلتر واقعی بین Requestها مشترک می‌شود؛ اگر فیلتر State قابل تغییر دارد (مثل Key/Value اینجا)، باید false بماند.
2. **فیلتر واقعی را حتماً در DI ثبت کنید:** ثبت با `AddTransient` منطقی‌ترین انتخاب است چون Factory خودش Key/Value/Order را بعد از ساخت ست می‌کند و Lifetime طولانی state را به‌اشتراک می‌گذارد.
3. **دو کلاس در یک نقش:** کلاس Factory «پیکربندی» است و کلاس فیلتر «رفتار»؛ این جداسازی دقیقاً همان الگوی Options Pattern است — مقایسه کنید.
4. **نام‌گذاری قراردادی:** نام کلاس با پسوند `Attribute` تعریف و بدون پسوند استفاده می‌شود؛ دیدن `[ResponseHeaderFilterFactory(...)]` یعنی همین کلاس Factory.

## ✅ خلاصه

`IFilterFactory` الگوی نهایی ساخت فیلترهای حرفه‌ای است: یک Attribute سبک برای گرفتن پیکربندی، و یک فیلترِ DI-محور برای اجرای رفتار. با آن، هم آرگومان‌دهی پروژه ۰۵ و هم DI پروژه ۱۸ و هم سینتکس تمیز پروژه ۱۹ یکجا جمع شد — به‌علاوه `ILogger` سالم. این الگو را در پروژه‌های واقعی زیاد خواهید دید.
