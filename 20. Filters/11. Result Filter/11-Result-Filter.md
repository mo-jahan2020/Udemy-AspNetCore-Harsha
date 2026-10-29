# پروژه ۱۱ — Result Filter (فیلتر نتیجه)

> منبع: پوشه `20. Filters/11. Result Filter`
> پیش‌نیاز: پروژه‌های ۰۹ و ۱۰ (فیلتر Async و الگوی `next()`)

---

## 🎯 مبحث این پروژه چیست؟

 Result Filter  چهارمین نوع فیلترِ معرفی‌شده است که دورِ اجرای نتیجه Action یعنی اجرای `IActionResult` مثل رندر View قرار می‌گیرد.
 یعنی بعد از آن‌که Action روش `return View(...)` را برگرداند و  قبل/حین رندر شدن View  اجرا می‌شود.

اینترفیس سنکرون: `IResultFilter` — و نسخه Async:  `IAsyncResultFilter`  با الگوی آشنای `await next()`:

```csharp
public async Task OnResultExecutionAsync(ResultExecutingContext context,
                                         ResultExecutionDelegate next)
{
    // قبل از اجرای نتیجه (رندر View)
    await next();
    // بعد از اجرای نتیجه (View رندر شد)
}
```

## 💡 چرا به این مبحث نیاز داریم؟

Action Filter مرحله Before/After «Action» را می‌گیرد، اما بعضی کارها به «خروجی نهایی» ربط دارند نه به منطق Action:

- افزودن Header به Response  بعد از  رندر (مثل `Last-Modified`)؛
- اصلاح یا بررسی محتوایی که View تولید می‌کند؛
- تصمیم درباره آیا اصلاً رندر انجام شود یا نه قطع زنجیره در سطح نتیجه.

اگر این کارها را در OnActionExecuted انجام دهید، دقیق نیست؛ چون آن‌جا View هنوز رندر نشده است. جای درست، Result Filter است.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ResultFilters/PersonsListResultFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
 public class PersonsListResultFilter : IAsyncResultFilter
 {
  private readonly ILogger<PersonsListResultFilter> _logger;

  public PersonsListResultFilter(ILogger<PersonsListResultFilter> logger)
  {
   _logger = logger;
  }

  public async Task OnResultExecutionAsync(ResultExecutingContext context,
                                           ResultExecutionDelegate next)
  {
   //TO DO: before logic
   _logger.LogInformation("{FilterName}.{MethodName} - before",
       nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync));

   await next(); // اجرای فیلتر نتیجه بعدی یا خود IActionResult (رندر View)

   //TO DO: after logic
   _logger.LogInformation("{FilterName}.{MethodName} - after",
       nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync));

   context.HttpContext.Response.Headers["Last-Modified"] =
       DateTime.Now.ToString("yyyy-MM-dd HH:mm");
  }
 }
}
```

## ⚙️ نحوه اتصال

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[action]")]
[Route("/")]
[TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { ..., 1 }, Order = 1)]

[TypeFilter(typeof(PersonsListResultFilter))]     // ← Result Filter جدید
public async Task<IActionResult> Index(...) { ... }
```

## 🧭 جایگاه در Pipeline کامل فیلترها

```text
Request
 → Authorization Filter   (پروژه ۱۳)
 → Resource Filter        (پروژه ۱۲)
 → Action Filter          (قبل از Action)   ← پروژه‌های ۰۱ تا ۱۰
 → خود Action Method
 → Action Filter          (بعد از Action)
 → Result Filter  ✅ همین‌جا — دورِ اجرای IActionResult
 → اجرای نتیجه: رندر View / نوشتن JSON / ...
 → Resource Filter (بعد)
Response
```

## 🧪 رفتار برنامه در اجرا

لاگ Serilog برای `persons/index`:

```text
PersonsListActionFilter.OnActionExecuting ...
PersonsListActionFilter.OnActionExecuted ...
PersonsListResultFilter.OnResultExecutionAsync - before
   (View رندر می‌شود)
PersonsListResultFilter.OnResultExecutionAsync - after
```

و در Headers پاسخ:

```text
Last-Modified: 2022-07-10 14:52
```

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IAsyncResultFilter` | اینترفیس Result Filter غیرهمزمان |
| `OnResultExecutionAsync(context, next)` | Before/After دور اجرای IActionResult |
| `ResultExecutingContext` | دسترسی به نتیجه (`context.Result`) و HttpContext قبل از رندر |
| `await next()` | ادامه زنجیره Result Filterها و اجرای نتیجه |
| `Response.Headers[...]` | تنظیم Header روی پاسخ نهایی |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  تفاوت با Action Filter:  After در Action Filter یعنی «Action برگشت» ولی View هنوز رندر نشده؛ After در Result Filter یعنی «نتیجه اجرا شد». اگر کد شما به محتوای رندرشده یا Header نهایی وابسته است، Result Filter انتخاب درست است.
2.  Result Filter روی Redirect هم اجرا می‌شود؟  بله اگر نتیجه `RedirectToActionResult` باشد، Result Filter دور «اجرای redirect» قرار می‌گیرد؛ اگر بخواهید فقط برای View خاصی منطق داشته باشید، نوع `context.Result` را چک کنید (مثلاً `is ViewResult`).
3.  Short-Circuit در این سطح:  اگر `context.Result` را قبل از `next()` عوض کنید یا اصلاً `next()` را نزنید،  رندر View انجام نمی‌شود  و پاسخ همان چیزی است که شما تعیین کردید — این ابزار پروژه ۱۵ است.
4.  اینترفیس سنکرون هم هست:  `IResultFilter` با دو متد `OnResultExecuting/OnResultExecuted`؛ همان قاعده قبلی: هر دو را پیاده کنید فقط Async اجرا می‌شود.

## ✅ خلاصه

Result Filter دور «اجرای نتیجه» (رندر View و...) قرار می‌گیرد و با `IAsyncResultFilter` و `await next()` پیاده می‌شود. در این پروژه فیلتر `PersonsListResultFilter` بعد از رندر، Header یعنی `Last-Modified` را روی پاسخ ثبت کرد. از این‌جا مجموعه فیلترها کامل‌تر می‌شود: بعدی Resource Filter است که از همه زودتر اجرا می‌شود.
