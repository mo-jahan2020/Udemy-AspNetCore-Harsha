# پروژه ۰۱ — Action Filter (فیلتر اقدام)

> منبع: پوشه `20. Filters/01. Action Filter` از مخزن Udemy-AspNetCore-Harsha
> پیش‌نیاز: آشنایی با Controller، Action Method و Dependency Injection (مباحث ۱۱ و ۱۵ دوره)

---

## 🎯 مبحث این پروژه چیست؟

**Action Filter** یکی از پنج نوع Filter در ASP.NET Core 
است که به شما اجازه می‌دهد منطقی را **قبل از اجرا**
(before) و **بعد از اجرا** (after) یک Action Method تزریق کنید 
— بدون آن‌که کد داخل خودِ Action را تغییر دهید.

Filter به‌طور کلی «قطعه‌کدی است که در نقاط مشخصی از پردازش
Request اجرا می‌شود» و برای پیاده‌سازی
**Cross-Cutting Concerns** (نگرانی‌های مشترک و تکرارشونده مثل 
Logging، اعتبارسنجی پارامترها، کش کردن و مدیریت خطا) به‌کار می‌رود. در این پروژه، اولین 
Action Filter دوره با نام `PersonsListActionFilter` ساخته می‌شود که ورود/خروج خود را در لاگ ثبت می‌کند.

## 💡 چرا به این مبحث نیاز داریم؟

تصور کنید می‌خواهید برای هر Action Method لاگ بنویسید یا پارامترهای ورودی آن را بررسی کنید. اگر این کار را داخل خود Action انجام دهید، همان کد را باید در ده‌ها Action تکرار کنید و کد Business Logic شما آلوده به کدهای جانبی می‌شود. Action Filter این منطق مشترک را در **یک کلاس مجزا** نگه می‌دارد و با یک Attribute به هر Action که بخواهید وصل می‌شود؛ به این ترتیب:

- کد Action تمیز می‌ماند و فقط کار اصلی خودش را انجام می‌دهد (اصل Single Responsibility).
- منطق مشترک در یک جا نگهداری می‌شود و اصلاح آن یک‌جا انجام می‌شود (اصل DRY).
- می‌توانید بدون دست‌زدن به Controller، رفتار جدیدی به Action اضافه کنید.

## 🧩 مفهوم Interface و دو متد اصلی

برای ساخت Action Filter سنکرون، یک کلاس می‌سازیم که اینترفیس **`IActionFilter`** را پیاده‌سازی کند. این اینترفیس دو متد دارد:

| متد | زمان اجرا | کاربرد رایج |
|---|---|---|
| `OnActionExecuting` | **قبل** از اجرای Action Method | اعتبارسنجی پارامترها، اصلاح ورودی‌ها، ثبت لاگ ورود |
| `OnActionExecuted` | **بعد** از اجرای Action Method | ثبت لاگ خروج، دست‌کاری نتیجه، تنظیم Header |

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/PersonsListActionFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
 public class PersonsListActionFilter : IActionFilter
 {
  private readonly ILogger<PersonsListActionFilter> _logger;

  // از طریق Constructor Injection سرویس لاگر تزریق می‌شود
  public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
  {
   _logger = logger;
  }

  // بعد از اجرای Action اجرا می‌شود
  public void OnActionExecuted(ActionExecutedContext context)
  {
   _logger.LogInformation("PersonsListActionFilter.OnActionExecuted method");
  }

  // قبل از اجرای Action اجرا می‌شود
  public void OnActionExecuting(ActionExecutingContext context)
  {
   _logger.LogInformation("PersonsListActionFilter.OnActionExecuting method");
  }
 }
}
```

نکته مهم این است که فیلتر یک کلاس معمولی C# است؛ چون به‌واسطه `TypeFilter` ساخته می‌شود، سازنده آن می‌تواند سرویس‌هایی مثل `ILogger<T>` را از DI Container دریافت کند (در ادامه دوره بیشتر با این مکانیزم آشنا می‌شوید).

## ⚙️ نحوه اتصال فیلتر به Action

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[action]")]
[Route("/")]
[TypeFilter(typeof(PersonsListActionFilter))]   // ← اتصال فیلتر به این Action
public async Task<IActionResult> Index(string searchBy, string? searchString, ...)
{
    _logger.LogInformation("Index action method of PersonsController");
    // ...
}
```

Attribute یعنی `[TypeFilter]` به ASP.NET Core می‌گوید: «هنگام اجرای این Action، یک نمونه از `PersonsListActionFilter` بساز (با رعایت سازنده و DI آن) و فیلتر را در Pipeline اجرا کن.» فیلترها را می‌توان در سه سطح اعمال کرد: **Action** (مثل همین‌جا)، **Controller** (روی همه Actionهای آن) و **Global** (روی کل برنامه — پروژه ۰۶).

## 🧪 رفتار برنامه در اجرا

وقتی کاربر صفحه `persons/index` را باز می‌کند، ترتیب اجرای متدها در خروجی لاگ (کنسول/Serilog) چنین است:

```text
info: PersonsListActionFilter.OnActionExecuting method   ← قبل از Action
info: Index action method of PersonsController           ← خود Action
info: PersonsListActionFilter.OnActionExecuted method    ← بعد از Action
```

این الگوی «قبل / خودکار / بعد» همان چیزی است که در تمام فیلترهای دوره تکرار می‌شود و پایه‌ی درک Pipeline فیلترهاست.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IActionFilter` | اینترفیس فیلتر Action سنکرون (دو متد Before/After) |
| `OnActionExecuting(ActionExecutingContext)` | کد قبل از Action |
| `OnActionExecuted(ActionExecutedContext)` | کد بعد از Action |
| `ActionExecutingContext` | دسترسی به پارامترها، Controller و HttpContext در مرحله Before |
| `ActionExecutedContext` | دسترسی به نتیجه Action و وضعیت اجرا در مرحله After |
| `[TypeFilter(typeof(...))]` | ساخت نمونه فیلتر با پشتیبانی از Constructor Injection |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **فیلتر سنکرون در برابر Async:** `IActionFilter` نسخه سنکرون است؛ از پروژه ۰۹ نسخه Async (`IAsyncActionFilter`) معرفی می‌شود. اگر هر دو نسخه را هم‌زمان پیاده کنید، فقط نسخه Async اجرا می‌شود.
2. **فراموش کردن Attribute اتصال:** اگر کلاس فیلتر را بنویسید اما آن را با `[TypeFilter]` به Action وصل نکنید، هیچ‌وقت اجرا نمی‌شود و هیچ خطایی هم نمی‌گیرید.
3. **چرا ILogger در سازنده؟** فیلتر قرار نیست با `new` دستی ساخته شود؛ سازنده به DI سپرده شده است، بنابراین می‌توانید هر سرویس ثبت‌شده‌ای را تزریق کنید.
4. **جای درست منطق:** منطقی که به «نتیجه View» نیاز دارد را در OnActionExecuting نگذارید؛ آن مرحله هنوز نتیجه‌ای وجود ندارد. (در پروژه ۱۱ با Result Filter آشنا می‌شوید که جای بهتری برای این کار است.)

## ✅ خلاصه

در این پروژه اولین Action Filter را ساختیم: کلاسی با پیاده‌سازی `IActionFilter` که با دو متد `OnActionExecuting` و `OnActionExecuted` منطق «قبل و بعد از Action» را در بر می‌گیرد و با `[TypeFilter]` به Action متصل می‌شود. این الگو، سنگ‌بنای تمام پروژه‌های بعدیِ فصل Filters است.
