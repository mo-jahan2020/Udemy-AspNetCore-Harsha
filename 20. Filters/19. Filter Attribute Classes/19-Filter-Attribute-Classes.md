# پروژه ۱۹ — Filter Attribute Classes (تبدیل فیلتر به Attribute)

> منبع: پوشه `20. Filters/19. Filter Attribute Classes`
> پیش‌نیاز: پروژه ۱۸ (ServiceFilter) و پروژه ۰۹ (فیلتر Async)

---

## 🎯 مبحث این پروژه چیست؟

تا حالا فیلترها کلاس‌هایی بودند که با `TypeFilter` یا `ServiceFilter` به Action وصل می‌شدند. این پروژه روش ساده‌تر و مستقیم‌تر را نشان می‌دهد: فیلتر را به یک **Attribute** تبدیل کنیم تا بتوان آن را مثل `[HttpGet]` مستقیماً نوشت:

```csharp
[ResponseHeaderActionFilter("MyKey-FromAction", "MyValue-From-Action", 1)]  // بدون TypeFilter!
```

کلید این تبدیل، ارث‌بری از **`ActionFilterAttribute`** است — کلاس پایه‌ای که خودش `IActionFilter`، `IAsyncActionFilter`، `IResultFilter`، `IAsyncResultFilter` و `IOrderedFilter` را هم‌زمان پیاده‌سازی کرده است و شما فقط متدهای موردنیاز را override می‌کنید.

## 💡 چرا به این مبحث نیاز داریم؟

سینتکس `[MyFilter(args)]` کوتاه‌تر، خواناتر و شبیه‌تر به اتریبیوت‌های استاندارد است. اما این راحتی یک **هزینه مهم** دارد که این پروژه به‌طور عمدی نشانش می‌دهد:

> **Attribute در .NET فقط می‌تواند در سازنده/پراپرتی‌های خود، ثابت‌های Compile-Time بگیرد (رشته، عدد، Type و...). DI اینجا کار نمی‌کند!**
> به همین دلیل `ILogger` که در نسخه‌های قبلی در سازنده تزریق می‌شد، در نسخه Attribute حذف شده است.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/ResponseHeaderActionFilter.cs` (نسخه Attribute)

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
 public class ResponseHeaderActionFilter : ActionFilterAttribute   // ← کلاس پایه Attribute
 {
  private readonly string _key;
  private readonly string _value;

  public ResponseHeaderActionFilter(string key, string value, int order)
  {
   _key = key;
   _value = value;
   Order = order;          // پراپرتی IOrderedFilter از ActionFilterAttribute ارث رسیده
  }

  public override async Task OnActionExecutionAsync(ActionExecutingContext context,
                                                    ActionExecutionDelegate next)
  {
   await next(); // فیلتر بعدی یا Action Method

   context.HttpContext.Response.Headers[_key] = _value;   // بعد از Action
  }
 }
}
```

> مقایسه کنید با پروژه ۰۸/۰۹: `ILogger` حذف شده چون Attribute نمی‌تواند از DI نمونه‌سازی شود. لاگ (موقتاً) فدای سینتکس ساده شده است — تا پروژه ۲۰ راه‌حل نهایی بیاید.

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[controller]")]
[TypeFilter(typeof(ResponseHeaderActionFilter), ...)]      // کد قدیمی کامنت شده
[ResponseHeaderActionFilter("My-Key-From-Controller", "My-Value-From-Controller", 3)]  // ← جدید
public class PersonsController : Controller { ... }

[ResponseHeaderActionFilter("MyKey-FromAction", "MyValue-From-Action", 1)]  // روی Action
public async Task<IActionResult> Index(...) { ... }
```

## 🧰 خانواده Attribute کلاس‌های پایه

| کلاس پایه | پوشش می‌دهد |
|---|---|
| `ActionFilterAttribute` | Action Filter (Sync+Async) + Result Filter (Sync+Async) + Order |
| `ResultFilterAttribute` | Result Filter (Sync+Async) |
| `ServiceFilterAttribute` | خودش ابزار اتصال است (پروژه ۱۸) |
| `TypeFilterAttribute` | ابزار اتصال با آرگومان (پروژه ۰۵) |
| `ExceptionFilterAttribute`، `ResourceFilterAttribute` | نسخه Attribute انواع دیگر |

## ⚙️ مقایسه سه روش اتصال (تا این جای دوره)

| روش | DI | آرگومان | سینتکس |
|---|---|---|---|
| `[TypeFilter(typeof(T), Arguments=...)]` | ✅ (سازنده) | ✅ | نسبتاً بلند |
| `[ServiceFilter(typeof(T))]` | ✅ کامل (ثبت در DI) | ❌ | کوتاه |
| `[T(args)]` (Attribute) | ❌ | ✅ (فقط ثابت) | کوتاه‌ترین |

## 🧪 رفتار برنامه در اجرا

مثل قبل: Header های تعریف‌شده روی Response هر Action ظاهر می‌شوند؛ فقط نمونه‌سازی فیلتر حالا توسط زیرساخت Attribute انجام می‌شود (یک نمونه برای هر استفاده، به‌ازای هر Request).

## ⚠️ نکات کلیدی و خطاهای رایج

1. **سرویس تزریق نکنید به سازنده Attribute:** کامپایلر اجازه نمی‌دهد یا اگر Type پاس دهید، خودتان باید resolve کنید — ضدالگو. راه‌حل‌های درست: ServiceFilter (پروژه ۱۸) یا IFilterFactory (پروژه ۲۰).
2. **ActionFilterAttribute ابرکلاس است:** اگر فقط override نکنید، پیاده‌سازی‌های پیش‌فرض خالی‌اش اجرا می‌شوند و مشکلی نیست؛ ولی مراقب باشید هم `OnActionExecuting` و هم `OnActionExecutionAsync` را override نکنید (فقط Async اجرا می‌شود).
3. **نام‌گذاری:** به‌طور قراردادی نام کلاس‌های Attribute به `Attribute` ختم می‌شود؛ هنگام استفاده می‌توانید پسوند را حذف کنید (`[ResponseHeaderActionFilter(...)]`). (در پروژه ۲۰ نام به `FilterFactoryAttribute` تغییر می‌کند.)
4. **Attribute ها Stateless هستند:** بین Requestها state نگه ندارید؛ هر Request نمونه تازه می‌سازد اما برای اطمینان، فیلد را mutable رها نکنید.

## ✅ خلاصه

با ارث‌بری از `ActionFilterAttribute`، فیلتر به یک Attribute واقعی تبدیل می‌شود و با سینتکس `[ResponseHeaderActionFilter("k","v",1)]` مستقیم استفاده می‌شود. هزینه‌اش از دست دادن DI در سازنده است — محدودیتی که در پروژه بعد با `IFilterFactory` به شکلی هوشمندانه حل می‌شود: بهترینِ هر دو دنیا.
