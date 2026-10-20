# پروژه ۰۸ — IOrderedFilter (پیاده‌سازی Order داخل خود فیلتر)

> منبع: پوشه `20. Filters/08. IOrderedFilter`
> پیش‌نیاز: پروژه ۰۷ (Order در Attribute)

---

## 🎯 مبحث این پروژه چیست؟

در پروژه قبل، `Order` را روی Attribute می‌نوشتیم.
این پروژه روش دوم و در بسیاری موارد بهتر را نشان می‌دهد: کلاس فیلتر، اینترفیس  `IOrderedFilter`  را پیاده‌سازی می‌کند 
و مقدار `Order` را از  سازنده  دریافت می‌کند. یعنی ترتیب، بخشی از خودِ فیلتر می‌شود — همراه با آرگومان‌های دیگر.

```csharp
public class ResponseHeaderActionFilter : IActionFilter, IOrderedFilter
{
 public int Order { get; }   // ← عضو اینترفیس IOrderedFilter

 public ResponseHeaderActionFilter(ILogger<...> logger, string key, string value, int order)
 {
  Order = order;   // ← از سازنده
  ...
 }
}
```

## 💡 چرا به این مبحث نیاز داریم؟

مزیت‌های `IOrderedFilter` نسبت به `Order` روی Attribute:

1.  ترتیب و پیکربندی یکجا:  سازنده فیلتر حالا کلید/مقدار/ترتیب را با هم می‌گیرد؛ فراخواننده نیازی به یادآوری پراپرتی جداگانه ندارد.
2.  کارکرد در همه محل‌های ثبت:  `IOrderedFilter` نه‌فقط با TypeFilter، بلکه برای فیلترهای  Global  (که Attribute ای در میان نیست) هم ترتیب می‌دهد:
   ```csharp
   options.Filters.Add(new ResponseHeaderActionFilter(logger, "K", "V", 2));
   ```
3.  پیش‌فرض قابل مدیریت:  می‌توانید مقدار پیش‌فرض Order را در کلاس تعیین کنید تا رفتار بدون پیکربندی هم معنادار باشد.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/ResponseHeaderActionFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
 public class ResponseHeaderActionFilter : IActionFilter, IOrderedFilter
 {
  private readonly ILogger<ResponseHeaderActionFilter> _logger;
  private readonly string Key;
  private readonly string Value;

  public int Order { get; }   // پیاده‌سازی IOrderedFilter

  public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger,
                                    string key, string value, int order)
  {
   _logger = logger;
   Key = key;
   Value = value;
   Order = order;
  }

  public void OnActionExecuting(ActionExecutingContext context)
  {
   _logger.LogInformation("{FilterName}.{MethodName} method",
       nameof(ResponseHeaderActionFilter), nameof(OnActionExecuting));
  }

  public void OnActionExecuted(ActionExecutedContext context)
  {
   _logger.LogInformation("{FilterName}.{MethodName} method",
       nameof(ResponseHeaderActionFilter), nameof(OnActionExecuted));

   context.HttpContext.Response.Headers[Key] = Value;
  }
 }
}
```

فایل: `CRUDExample/Program.cs` (Global هم حالا Order دارد)

```csharp
builder.Services.AddControllersWithViews(options => {
 var logger = builder.Services.BuildServiceProvider()
     .GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

 // ترتیبِ فیلتر سراسری، حالا از سازنده می‌آید
 options.Filters.Add(new ResponseHeaderActionFilter(logger, "My-Key-From-Global", "My-Value-From-Global", 2));
});
```

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[controller]")]
[TypeFilter(typeof(ResponseHeaderActionFilter),
    Arguments = new object[] { "My-Key-From-Controller", "My-Value-From-Controller", 3 },
    Order = 3)]      // هر دو راه: سازنده و Attribute
public class PersonsController : Controller { ... }

[TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
[TypeFilter(typeof(ResponseHeaderActionFilter),
    Arguments = new object[] { "MyKey-FromAction", "MyValue-From-Action", 1 },
    Order = 1)]
public async Task<IActionResult> Index(...) { ... }
```

## ⚙️ اولویت وقتی هر دو Order وجود دارد

اگر هم در سازنده Order داده شده باشد و هم روی Attribute،  مقدار Attribute برنده است  و مقدار سازنده را بازنویسی می‌کند. برای همین در کد بالا کنار هم نوشته شده‌اند تا تفاوت دیده شود.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IOrderedFilter` | اینترفیس با تک‌پراپرتی `int Order { get; }` |
| `Order` (سازنده) | ترتیبِ پیش‌فرضِ همراه فیلتر، بدون نیاز به Attribute |
| `Order` (Attribute) | بازنویسی در محل استفاده (اولویت بالاتر) |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  فراموش نکنید اینترفیس را در تعریف کلاس بنویسید:  اگر `IOrderedFilter` را اضافه نکنید، پراپرتی `Order` فقط یک پراپرتی معمولی است و Pipeline آن را نمی‌خواند.
2.  کد تکراری Order دو جا گمراه‌کننده است:  توصیه دوره این است که یک منبع حقیقت (Source of Truth) داشته باشید؛ معمولاً سازنده برای فیلترهای Global/ServiceFilter و Attribute برای ActionFilter های آدرس‌محور.
3.  `Order` در Attribute همچنان کار می‌کند:  پروژه ۰۷ را دور نیندازید؛ دو روش مکمل‌اند و باهم سازگارند.
4.  یادداشت طراحی:  زمانی که Order «ماهیت» فیلتر است (مثلاً «این فیلتر همیشه آخر باید Header بنویسد»)، تعبیه آن در کلاس با IOrderedFilter تمیزتر از پخش‌کردن اعداد در Controllerهاست.

## ✅ خلاصه

`IOrderedFilter` ترتیب را به بخشی از هویت فیلتر تبدیل می‌کند: کلاس پراپرتی `Order` را از سازنده می‌گیرد و هم در `[TypeFilter]` و هم در ثبت سراسری معتبر است. اگر Attribute هم `Order` بدهد، آن مقدار غالب می‌شود. با این پروژه ابزار ترتیب‌دهی کامل شد و از پروژه بعد به دنیای  فیلترهای Async  می‌رویم.
