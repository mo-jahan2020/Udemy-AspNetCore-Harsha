# پروژه ۱۲ — Resource Filter (فیلتر منبع)

> منبع: پوشه `20. Filters/12. Resource Filter`
> پیش‌نیاز: پروژه ۱۱ (الگوی `await next()` در فیلترها)

---

## 🎯 مبحث این پروژه چیست؟

**Resource Filter** نوعی فیلتر است که **دورِ کل Pipeline فیلترها** قرار می‌گیرد — بعد از Authorization Filter (که هنوز معرفی نشده)، و قبل از همه Action/Result Filterها. این یعنی اولین فیلتری است که کنترل را به دست می‌گیرد و آخرین فیلتری است که برمی‌گردد.

اینترفیس‌ها: `IResourceFilter` (سنکرون) و **`IAsyncResourceFilter`** (غیرهمزمان):

```csharp
public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
                                           ResourceExecutionDelegate next)
{
    // قبل از همه چیز (Model Binding هم هنوز انجام نشده!)
    await next();
    // بعد از همه چیز (پاسخ آماده است)
}
```

در این پروژه فیلتر `FeatureDisabledResourceFilter` ساخته می‌شود که می‌تواند یک قابلیت را **غیرفعال** کند و برای درخواست‌ها کد وضعیت ۵۰۱ برگرداند.

## 💡 چرا به این مبحث نیاز داریم؟

چون Resource Filter **قبل از Model Binding و قبل از هر فیلتر دیگر** اجرا می‌شود، برای کارهایی عالی است که باید «خیلی زود» یا «روی کل جریان» تصمیم بگیرند:

- **Feature Flag / خاموش کردن موقت قابلیت:** مثل این پروژه — حین خرابی یا تعمیر، Create را با 501 پاس بده؛
- **Caching کوتاه‌مدت Response** (اگر کش معتبر بود، اصلاً به Action نرس)؛
- اجرای منطقی که باید قبل از Model Binding انجام شود.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ResourceFilters/FeatureDisabledResourceFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResourceFilters
{
 public class FeatureDisabledResourceFilter : IAsyncResourceFilter
 {
  private readonly ILogger<FeatureDisabledResourceFilter> _logger;
  private readonly bool _isDisabled;

  public FeatureDisabledResourceFilter(ILogger<FeatureDisabledResourceFilter> logger,
                                       bool isDisabled = true)
  {
   _logger = logger;
   _isDisabled = isDisabled;
  }

  public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
                                             ResourceExecutionDelegate next)
  {
   //before
   _logger.LogInformation("{FilterName}.{MethodName} - before",
       nameof(FeatureDisabledResourceFilter), nameof(OnResourceExecutionAsync));

   if (_isDisabled)
   {
    //context.Result = new NotFoundResult();        // 404 - Not Found
    context.Result = new StatusCodeResult(501);     // 501 - Not Implemented
                                                    // ← زنجیره قطع می‌شود
   }
   else
   {
    await next();                                   // ادامه Pipeline
   }

   //after
   _logger.LogInformation("{FilterName}.{MethodName} - after",
       nameof(FeatureDisabledResourceFilter), nameof(OnResourceExecutionAsync));
  }
 }
}
```

## ⚙️ نحوه اتصال

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[action]/{personID}")]
[HttpPost]
[TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
[TypeFilter(typeof(FeatureDisabledResourceFilter),
    Arguments = new object[] { false })]   // ← قابلیت فعال است (false = not disabled)
public async Task<IActionResult> Edit(PersonUpdateRequest personRequest) { ... }
```

با `false` قابلیت روشن است؛ برای خاموش کردن Edit فقط کافی است آرگومان را `true` کنید — بدون تغییر هیچ کد دیگری.

## 🧭 جایگاه در Pipeline

```text
Request (Middlewareها)
 → Authorization Filter   (پروژه ۱۳)
 → Resource Filter  ✅ همین‌جا (قبل از Model Binding)
    → Action Filter → Action → Action Filter
    → Result Filter → اجرای نتیجه (رندر View)
 → Resource Filter (بعد از همه)
Response
```

## 🧪 رفتار برنامه در اجرا

| مقدار `isDisabled` | نتیجه Submit فرم Edit |
|---|---|
| `false` | Action عادی اجرا می‌شود |
| `true` | پاسخ **501 Not Implemented**؛ هیچ Action/Result/View ای اجرا نمی‌شود |

نکته جالب در لاگ: وقتی قطع (Short-Circuit) می‌شود، هر دو پیام `before` و `after` فیلتر چاپ می‌شوند — چون `after` بعد از بلوک `if` است و در هر حال اجرا می‌شود.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IAsyncResourceFilter` | اینترفیس Resource Filter غیرهمزمان |
| `OnResourceExecutionAsync(context, next)` | دور کل Pipeline فیلترها |
| `ResourceExecutingContext` | قبل از Model Binding؛ فقط.HttpContext و RouteData در دسترس است |
| `new StatusCodeResult(501)` / `NotFoundResult()` | پاس‌های قطع‌کننده زنجیره |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **پارامترهای Action اینجا نیستند:** در `ResourceExecutingContext` چیزی مثل `ActionArguments` وجود ندارد، چون Model Binding هنوز انجام نشده. اگر به پارامترها نیاز دارید، Action Filter بنویسید.
2. **Short-Circuit قوی‌ترین نوع:** قطع در Resource Filter یعنی همه چیز بعد از آن (Action Filter، Action، Result Filter، رندر) اجرا نمی‌شود — قوی‌ترین نقطه قطع در Pipeline فیلترها.
3. **کد وضعیت معنادار انتخاب کنید:** 501 یعنی «سرور این قابلیت را پیاده/فعال نکرده» و برای Feature Flag مناسب‌تر از 404 است که معنایش «صفحه وجود ندارد» است.
4. **حتی بدون قطع، بهینه‌سازی کنید:** اگر فیلتر Resource فقط لاگ می‌نویسد، بدانید که روی **هر Request** اجرا می‌شود؛ کدش را سبک نگه دارید.

## ✅ خلاصه

Resource Filter دور کل Pipeline فیلترها می‌پیچد: اولین نقطه کنترل بعد از Authorization و آخرین نقطه قبل از خروج. `FeatureDisabledResourceFilter` الگوی Feature Flag را با `StatusCodeResult(501)` پیاده کرد. با این فیلتر، چهار نوع از پنج نوع اصلی فیلتر معرفی شد؛ بعدی Authorization Filter است که حتی از Resource هم جلوتر است.
