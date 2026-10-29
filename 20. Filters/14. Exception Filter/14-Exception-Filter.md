# پروژه ۱۴ — Exception Filter (فیلتر استثنا)

> منبع: پوشه `20. Filters/14. Exception Filter`
> پیش‌نیاز: پروژه‌های ۱۲ و ۱۳ (الگوی قطع زنجیره و Pipeline کامل)

---

## 🎯 مبحث این پروژه چیست؟

 Exception Filter  پنجمین نوع اصلی فیلتر است و آخرین حلقه‌ی Pipeline:
 هرگاه داخل Action، فیلترهای دیگر یا رندر نتیجه یک  استثنای مدیریت‌نشده  (Unhandled Exception) رخ دهد،
 Exception Filter فرصت پیدا می‌کند آن را ببیند، لاگ کند و پاسخ مناسبی تولید کند.

اینترفیس:  `IExceptionFilter`  (فقط متد `OnException`). در این پروژه فیلتر `HandleExceptionFilter` ساخته می‌شود که:

1. نوع و پیام استثنا را با لاگ ساختاریافته ثبت می‌کند؛
2. در محیط Development، پیام استثنا را به‌صورت متن با کد 500 برمی‌گرداند.

## 💡 چرا به این مبحث نیاز داریم؟

بدون Exception Filter، استثنای داخل Action به Middlewareهای بالاتر (Developer Exception Page یا Exception Handler) 
می‌رود. اما فیلترها این مزیت را دارند که:

-  نزدیک به نقطه خطا  هستند و می‌توانند خطا را لاگ کنند؛
- می‌توانند بسته به نوع استثنا یا محیط،  پاسخ سفارشی  برگردانند؛
- بدون گم شدن خطا، از Crash کردن ناخوشایند Response جلوگیری کنند.

تفاوت با فصل Error Handling (پروژه ۲۱ دوره): Exception Filter فقط خطاهای داخل MVC Pipeline را می‌گیرد؛
خطاهای Middleware-level را نه. برای پوشش کامل، هر دو لازم‌اند.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ExceptionFilters/HandleExceptionFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ExceptionFilters
{
 public class HandleExceptionFilter : IExceptionFilter
 {
  private readonly ILogger<HandleExceptionFilter> _logger;
  private readonly IHostEnvironment _hostEnvironment;

  public HandleExceptionFilter(ILogger<HandleExceptionFilter> logger,
                               IHostEnvironment hostEvironment)
  {
   _logger = logger;
   _hostEnvironment = hostEvironment;
  }

  public void OnException(ExceptionContext context)
  {
   // لاگ ساختاریافته: نوع و پیام استثنا
   _logger.LogError(
       "Exception filter {FilterName}.{MethodName}\n{ExceptionType}\n{ExceptionMessage}",
       nameof(HandleExceptionFilter), nameof(OnException),
       context.Exception.GetType().ToString(), context.Exception.Message);

   // فقط در محیط Development پاسخ سفارشی بده
   if (_hostEnvironment.IsDevelopment())
    context.Result = new ContentResult()
    {
     Content = context.Exception.Message,
     StatusCode = 500
    };
  }
 }
}
```

## ⚙️ نحوه اتصال

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[controller]")]
[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { ..., 3 }, Order = 3)]
[TypeFilter(typeof(HandleExceptionFilter))]    // ← روی کل Controller
public class PersonsController : Controller { ... }
```

چون در سطح Controller ثبت شده، هر استثنایی که در  هر Action از این Controller  رخ دهد به آن می‌رسد (می‌توانستیم Global هم ثبتش کنیم).

## 🧪 رفتار برنامه در اجرا

سناریوی تست (مثلاً خطای دیتابیس در حین GetFilteredPersons):

```text
فیلتر فعال می‌شود:
fail: Exception filter HandleExceptionFilter.OnException
      System.NullReferenceException
      Object reference not set to an instance of an object.

در Development: صفحه‌ای با متن پیام استثنا و کد وضعیت 500
در Production:  نتیجه‌ای ست نمی‌شود → استثنا طبق روال به Middlewareها می‌رود
```

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IExceptionFilter` | اینترفیس فیلتر استثنا (متد OnException) |
| `ExceptionContext.Exception` | خود استثنای رخ‌داده (Type و Message) |
| `context.Result = ...` | تعیین پاسخ؛ ست کردن آن یعنی «خطا مدیریت شد» |
| `IHostEnvironment.IsDevelopment()` | تصمیم محیط‌محور (فصل Environments دوره) |
| `ContentResult { StatusCode = 500 }` | پاسخ متنی با کد وضعیت 500 |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  `ExceptionHandled` را فراموش نکنید:  در این مثال، ست‌کردن `context.Result` عملاً باعث مدیریت خطا می‌شود؛ اگر فقط لاگ کنید و Result ندهید، استثنا به Middlewareها ادامه می‌یابد. برای قطع قطعی، `context.ExceptionHandled = true` را هم ست کنید.
2.  Exception Filter خطاهای Async را هم می‌گیرد:  استثنای داخل فیلترهای Async (قبل یا بعد از `next()`) و داخل رندر View هم توسط همین فیلتر گرفته می‌شود.
3.  در Production چه کنیم؟  این فیلتر در Production عمداً پاسخ سفارشی نمی‌دهد تا جزئیات خطا فاش نشود؛ مدیریت کامل Production در فصل Error Handling با `UseExceptionHandler` انجام می‌شود. تکه‌کد بالا الگوی «رفتار متفاوت بر اساس محیط» را آموزش می‌دهد.
4.  محدودیت مهم:  Exception Filter خطاهای Middleware (مثل خطای در `UseStaticFiles` یا مسیریابی) را نمی‌گیرد — فقط خطاهای داخل MVC (Action/Filter/Result) را.

## ✅ خلاصه

با `IExceptionFilter.OnException` پنجمین نوع فیلتر معرفی شد: نقطه‌ای که استثناهای Pipeline را می‌بیند، لاگ می‌کند و می‌تواند با `context.Result` پاسخ سفارشی بسازد. `HandleExceptionFilter` الگوی «لاگ همیشه + پاسخ فقط در Development» را پیاده کرد. حالا هر پنج نوع فیلتر شناخته شده‌اند؛ پروژه بعدی به‌صورت عملی «اثر Short-Circuiting» روی این Pipeline را نشان می‌دهد.
