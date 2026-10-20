# پروژه ۰۹ — Async Filters (فیلترهای غیرهمزمان)

> منبع: پوشه `20. Filters/09. Async Filters`
> پیش‌نیاز: پروژه‌های ۰۵ تا ۰۸ و آشنایی با `async/await` در C#

---

## 🎯 مبحث این پروژه چیست؟

تا این‌جا فیلترها با دو متد جدا (`OnActionExecuting` و `OnActionExecuted`) نوشته می‌شدند.
این پروژه نسخه  Async  را معرفی می‌کند: اینترفیس  `IAsyncActionFilter`  که فقط  یک متد  دارد و هر دو فاز
Before/After را در همان یک متد حول یک فراخوانی `await next()` جمع می‌کند:

```csharp
public async Task OnActionExecutionAsync(
    ActionExecutingContext context,      // ورودی: قبل از Action
    ActionExecutionDelegate next)        // دلیگیت: ادامه Pipeline
{
    // کد قبل از Action
    await next();   // ← فیلترهای بعدی + خود Action اجرا می‌شوند
    // کد بعد از Action
}
```

## 💡 چرا به این مبحث نیاز داریم؟

1.  عملیات غیرهمزمان:  اگر فیلتر بخواهد از دیتابیس بخواند (مثل `GetAllCountries`) یا HTTP Call بزند، نسخه سنکرون Thread را بلاک می‌کند و در ترافیک بالا مقیاس‌پذیر نیست.
2.  مدل ذهنی ساده‌تر و نزدیک‌تر به Middleware:  الگوی `await next()` دقیقاً همان الگوی Middleware است؛ منطق Before/After در یک نگاه دیده می‌شود.
3.  دروازه Short-Circuiting:  قابلیت مهمی که در پروژه ۱۰ با همین اینترفیس پیاده می‌شود (عدم فراخوانی `next()`).
4.  اولویت اجرا:  اگر کلاس هم `IActionFilter` و هم `IAsyncActionFilter` را پیاده کند، فقط نسخه Async اجرا می‌شود.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/ResponseHeaderActionFilter.cs` (نسخه Async)

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
 public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
 {
  private readonly ILogger<ResponseHeaderActionFilter> _logger;
  private readonly string _key;
  private readonly string _value;

  public int Order { get; }

  public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger,
                                    string key, string value, int order)
  {
   _logger = logger;
   _key = key;
   _value = value;
   Order = order;
  }

  public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                           ActionExecutionDelegate next)
  {
   _logger.LogInformation("{FilterName}.{MethodName} method - before",
       nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

   await next(); // اجرای فیلتر بعدی یا خود Action Method

   _logger.LogInformation("{FilterName}.{MethodName} method - after",
       nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

   context.HttpContext.Response.Headers[_key] = _value;   // بعد از Action
  }
 }
}
```

> توجه: همان `[TypeFilter]` قبلی بدون هیچ تغییری کار می‌کند؛ ASP.NET Core خودش تشخیص می‌دهد فیلتر Async است.

## ⚙️ مقایسه دو مدل

| ویژگی | Sync (`IActionFilter`) | Async (`IAsyncActionFilter`) |
|---|---|---|
| تعداد متد | ۲ متد | ۱ متد |
| مرز Before/After | دو متد جدا | قبل/بعد از `await next()` |
| عملیات I/O غیرهمزمان | ❌ بلاک Thread | ✅ مناسب |
| Short-Circuiting | `context.Result = ...` بدون صدا زدن متد After |  ننوشتن `await next()`  (یا Return قبل از آن) |
| توصیه رسمی | برای کارهای سبک CPU | برای فیلترهای دارای I/O |

## 🧪 رفتار برنامه در اجرا

خروجی لاگ برای `Index`:

```text
ResponseHeaderActionFilter.OnActionExecutionAsync method - before
  → Action Method اجرا می‌شود
ResponseHeaderActionFilter.OnActionExecutionAsync method - after
```

و Header همچنان روی Response تنظیم می‌شود. رفتار بیرونی با پروژه ۰۸ یکسان است؛ فقط مدل داخلی تغییر کرده.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IAsyncActionFilter` | اینترفیس فیلتر Action غیرهمزمان |
| `OnActionExecutionAsync(context, next)` | متد یکپارچه Before/Action/After |
| `ActionExecutionDelegate next` | ادامه دادن Pipeline (فیلتر بعدی یا Action) |
| `await next()` | اجبار به ادامه؛ نبودن آن = قطع زنجیره (پروژه بعد) |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  فراموش کردن `await next()` = Short-Circuit ناخواسته:  اگر `next()` را صدا نزنید، Action اصلاً اجرا نمی‌شود و صفحه خالی/خطا می‌بینید. اگر عمداً قطع می‌کنید، حتماً `context.Result` را تنظیم کنید (پروژه ۱۰).
2.  دوبار صدا زدن `next()` ممنوع:  فقط یک‌بار `await next()` بزنید؛ فراخوانی دوم رفتار تعریف‌نشده/خطا دارد.
3.  Exception درون فیلتر Async:  خطایی که قبل از `next()` رخ دهد مثل «Before» است و کل Request را قطع می‌کند؛ خطای بعد از `next()` به فازهای Return برمی‌گردد — Exception Filter (پروژه ۱۴) هر دو را می‌گیرد.
4.  قانون انتخاب:  ASP.NET Core توصیه می‌کند اگر فیلتر منطق غیرهمزمان دارد، حتماً نسخه Async بنویسید؛ اگر Sync و Async با هم پیاده شوند، فقط Async اجرا می‌شود.

## ✅ خلاصه

`IAsyncActionFilter` فیلترِ تک‌متدی با الگوی `await next()` است: کد قبل از فراخوانی next = Before، بعد از آن = After. این الگو هم کارایی I/O را حل می‌کند، هم همان مدل ذهنی Middleware را می‌دهد و هم Short-Circuiting را ممکن می‌کند. با همین ابزار در پروژه بعد اولین فیلتر واقعی و کاربردی دوره را می‌سازیم.
