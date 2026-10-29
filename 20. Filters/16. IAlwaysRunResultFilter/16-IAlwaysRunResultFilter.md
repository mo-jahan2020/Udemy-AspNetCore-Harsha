# پروژه ۱۶ — IAlwaysRunResultFilter (فیلتر نتیجه همیشه‌اجرا)

> منبع: پوشه `20. Filters/16. IAlwaysRunResultFilter`
> پیش‌نیاز: پروژه ۱۵ (اثر Short-Circuiting روی Result Filterها)

---

## 🎯 مبحث این پروژه چیست؟

در پروژه ۱۵ دیدیم که وقتی زنجیره قطع می‌شود، Result Filterهای معمولی ممکن است از اجرا جا بمانند
.  `IAlwaysRunResultFilter`  راه‌حل این مشکل است: اینترفیس‌ای که رفتارش «همیشه اجرا شدن» است — حتی وقتی فیلترهای دیگر به‌خاطر
Authorization/Resource short-circuit یا Exception از دور خارج شده‌اند.

در این پروژه فیلتر `PersonAlwaysRunResultFilter` در سطح Controller اضافه می‌شود و 
`[TypeFilter(typeof(TokenResultFilter))]` روی Create GET کامنت می‌شود تا تفاوت رفتار قابل مشاهده باشد:

```csharp
//۹۲: //[TypeFilter(typeof(TokenResultFilter))]   ← خاموش شد
```

## 💡 چرا به این مبحث نیاز داریم؟

سناریوی واقعی: فرض کنید به‌ازای هر پاسخی — حتی خطای 401 یا 501 یا پاسخ Exception — باید کاری انجام شود: ست کردن کوکی CSRF، افزودن Header امنیتی، لاگ پاسخ، یا تمیزکاری منابع. Result Filter معمولی (`IResultFilter`) تضمینی برای اجرا در این مسیرها ندارد؛ `IAlwaysRunResultFilter` این تضمین را می‌دهد.

تفاوت دقیق دو اینترفیس:

| اینترفیس | اجرا در مسیر عادی | اجرا بعد از Short-Circuit | اجرا بعد از Exception |
|---|---|---|---|
| `IResultFilter` / `IAsyncResultFilter` | ✅ | ❌ (معمولاً جا می‌ماند) | ❌ |
| `IAlwaysRunResultFilter` | ✅ | ✅ | ✅ |

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ResultFilters/PersonAlwaysRunResultFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
 public class PersonAlwaysRunResultFilter : IAlwaysRunResultFilter
 {
  public void OnResultExecuted(ResultExecutedContext context)
  {
  }

  public void OnResultExecuting(ResultExecutingContext context)
  {
   //TO DO: before logic here
  }
 }
}
```

> این نسخه اولیه خالی است تا در کلاس، جایگاه API توضیح داده شود؛ در پروژه ۱۷ همین فیلتر شرط Skip را می‌گیرد.

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[controller]")]
[TypeFilter(typeof(ResponseHeaderActionFilter), ..., Order = 3)]
[TypeFilter(typeof(HandleExceptionFilter))]
[TypeFilter(typeof(PersonAlwaysRunResultFilter))]   // ← سطح Controller: همیشه اجرا
public class PersonsController : Controller { ... }
```

## ⚙️ متدها و Context ها

| متد | زمان اجرا | Context | کاربرد |
|---|---|---|---|
| `OnResultExecuting(ResultExecutingContext)` | قبل از اجرای نتیجه | می‌تواند `context.Result`/`Cancel` را تغییر دهد | تنظیم Header/کوکی روی پاسخِ در حال ساخت |
| `OnResultExecuted(ResultExecutedContext)` | بعد از اجرای نتیجه | `context.Canceled` و `Exception` در دسترس | لاگ نهایی، پاک‌سازی |

## 🧪 رفتار برنامه در اجرا

- `Index` (مسیر عادی): هم Result Filter معمولی و هم AlwaysRun اجرا می‌شوند.
- مسیرهای قطع‌شده (مثلاً 401 از Authorization در Edit): Result Filter معمولی اجرا نمی‌شود ولی `PersonAlwaysRunResultFilter.OnResultExecuting` اجرا می‌شود — و این دقیقاً تفاوت قابل مشاهده این پروژه است.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IAlwaysRunResultFilter` | اینترفیس نتیجه «همیشه‌اجرا» (سنکرون؛ نسخه Async ندارد) |
| `OnResultExecuting / OnResultExecuted` | Before/After دور اجرای نتیجه در همه مسیرها |
| `ResultExecutedContext.Canceled` | نشان می‌دهد آیا اجرای نتیجه قطع شده یا نه |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  همیشه ≠ بی‌شرط منطقی:  «همیشه اجرا» به معنی اجرا در همه مسیرهای Pipeline است؛ اگر خودتان داخل متد شرط گذاشته باشید (مثل پروژه ۱۷)، آن شرط همچنان می‌تواند اجرا را رد کند.
2.  حجم کار را کم نگه دارید:  چون این فیلتر در تمام پاسخ‌ها (حتی خطاها) اجرا می‌شود، هر خطی که این‌جا بنویسید روی کل برنامه اثر می‌گذارد؛ مناسب Headerها/لاگ سبک، نامناسب کارهای سنگین.
3.  دو Result Filter با هم:  `PersonsListResultFilter` (معمولی) و `PersonAlwaysRunResultFilter` می‌توانند هم‌زمان فعال باشند؛ ترتیب‌شان با قواعد Order مدیریت می‌شود.
4.  سنکرون است:  برخلاف `IAsyncResultFilter`، نسخه Async برای AlwaysRun وجود ندارد؛ کارها را سبک نگه دارید.

## ✅ خلاصه

`IAlwaysRunResultFilter` نسخه «تضمین‌شده» فیلتر نتیجه است: در مسیر عادی، بعد از Short-Circuit و حتی بعد از Exception اجرا می‌شود. برای منطق‌هایی که باید روی هر پاسخ اعمال شوند (Header امنیتی، لاگ پاسخ، پاک‌سازی) انتخاب درست است. در پروژه بعد همین فیلتر یاد می‌گیرد خودش را با یک Attribute سفارشی از اجرا معاف کند — مفهوم Filter Overrides.
