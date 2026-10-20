# پروژه ۰۵ — Filter Arguments  ارسال آرگومان به فیلتر 

> منبع: پوشه `20. Filters/05. Filter Arguments`
> پیش‌نیاز: پروژه‌های ۰۱ تا ۰۴

---

## 🎯 مبحث این پروژه چیست؟

تا حالا فیلتر ما پارامتری از بیرون نمی‌گرفت و مقادیرش ثابت بود. در این پروژه یک  فیلتر دوم با نام
`ResponseHeaderActionFilter`  ساخته می‌شود که در سازنده خود  آرگومان‌های ورودی   کلید و مقدار
Header  می‌گیرد و یاد می‌گیریم هنگام اتصال فیلتر با `[TypeFilter]`، این آرگومان‌ها را چگونه ارسال کنیم.

سناریو: می‌خواهیم به Response چند Header سفارشی اضافه کنیم .
برای هر Action با مقادیر متفاوت. پس فیلتر باید قابل تنظیم باشد، نه هاردکد.

## 💡 چرا به این مبحث نیاز داریم؟

قدرت واقعی فیلترها در  قابلیت استفاده مجدد  Reusability   است. اگر فیلتر فقط برای یک
Action خاص و با مقادیر ثابت کار کند، ارزشی ندارد.
با Filter Arguments می‌توانید یک فیلتر عمومی بنویسید  مثلاً «افزودن
Header»  و آن را با پیکربندی متفاوت به ده‌ها Action وصل کنید:

```csharp
[TypeFilter typeof ResponseHeaderActionFilter , Arguments = new object[] { "key-A", "value-A" } ]
[TypeFilter typeof ResponseHeaderActionFilter , Arguments = new object[] { "key-B", "value-B" } ]
```

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/ResponseHeaderActionFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
 public class ResponseHeaderActionFilter : IActionFilter
 {
  private readonly ILogger<ResponseHeaderActionFilter> _logger;
  private readonly string Key;
  private readonly string Value;

  // آرگومان‌ها از طریق سازنده دریافت می‌شوند
  public ResponseHeaderActionFilter ILogger<ResponseHeaderActionFilter> logger, string key, string value 
  {
   _logger = logger;
   Key = key;
   Value = value;
  }

  //before
  public void OnActionExecuting ActionExecutingContext context 
  {
   _logger.LogInformation "{FilterName}.{MethodName} method",
       nameof ResponseHeaderActionFilter , nameof OnActionExecuting  ;
  }

  //after
  public void OnActionExecuted ActionExecutedContext context 
  {
   _logger.LogInformation "{FilterName}.{MethodName} method",
       nameof ResponseHeaderActionFilter , nameof OnActionExecuted  ;

   // تنظیم Header روی Response — بعد از اجرای Action
   context.HttpContext.Response.Headers[Key] = Value;
  }
 }
}
```

## ⚙️ نحوه ارسال آرگومان‌ها — `Arguments` در TypeFilter

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route "[action]" ]
[Route "/" ]
[TypeFilter typeof PersonsListActionFilter  ]
[TypeFilter typeof ResponseHeaderActionFilter ,
    Arguments = new object[] { "X-Custom-Key", "Custom-Value" } ]   // ← آرگومان‌های فیلتر
public async Task<IActionResult> Index ... 
{ ... }

[Route "[action]" ]
[HttpGet]
[TypeFilter typeof ResponseHeaderActionFilter ,
    Arguments = new object[] { "my-key", "my-value" } ]             // همان فیلتر، پیکربندی دیگر
public async Task<IActionResult> Create ... 
{ ... }
```

مکانیزم کار `[TypeFilter]`:

1. ASP.NET Core می‌خواهد نمونه‌ای از `ResponseHeaderActionFilter` بسازد.
2. اولین پارامتر  `ILogger<T>`  از  DI Container  حل می‌شود.
3. بقیه پارامترها  `key` و `value`  از آرایه  `Arguments`  به ترتیب به سازنده پاس داده می‌شوند.
4. فیلتر اجرا می‌شود و در `OnActionExecuted` مقدار `Response.Headers[Key] = Value` را تنظیم می‌کند.

## 🧪 رفتار برنامه در اجرا

پس از باز کردن `persons/index`، در DevTools مرورگر  تب Network → Headers  می‌بینید:

```text
X-Custom-Key: Custom-Value
```

و برای `persons/create`:

```text
my-key: my-value
```

همچنین در لاگ Serilog ترتیب اجرا دیده می‌شود: `OnActionExecuting` → Action → `OnActionExecuted`  که Header را ثبت می‌کند .

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `[TypeFilter typeof T , Arguments = object[] ]` | ساخت فیلتر با ترکیب سرویس‌های DI + آرگومان‌های دستی |
| `Response.Headers[Key] = Value` | افزودن/بازنویسی یک Header روی Response |
| `OnActionExecuted` | جای درست برای کاری که باید بعد از Action انجام شود  مثل تنظیم Header  |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  ترتیب آرگومان‌ها مهم است:  عناصر `Arguments` به ترتیب به پارامترهای سازنده نگاشت می‌شوند؛ اگر ترتیب را جابه‌جا کنید، کلید و مقدار جابه‌جا می‌شوند و خطای Runtime نمی‌گیرید — فقط نتیجه غلط است.  در این مثال اولی ILogger است که از DI می‌آید و در Arguments نمی‌آید. 
2.  فقط نوع‌های ساده را در Arguments بگذارید:  چون در Attribute نوشته می‌شود، مقادیر باید Literal قابل نوشتن باشند  رشته، عدد، bool و... . سرویس‌ها را در Arguments نگذارید؛ آن‌ها را به DI بسپارید تا خودکار تزریق شوند.
3.  هر `[TypeFilter]` یک نمونه جدا می‌سازد:  اگر دو بار همین فیلتر را با Arguments متفاوت بگذارید، دو نمونه مستقل خواهید داشت و هر دو اجرا می‌شوند — این رفتار عمدی و مطلوب است.
4.  چرا تنظیم Header در After و نه Before؟  چون Header باید روی Response نهایی تنظیم شود و در مرحله قبل، Action ممکن است Response را بازنویسی/redirect کند.  در پروژه ۱۱ همین کار با Result Filter هم انجام می‌شود که جای طبیعی‌تری است. 

## ✅ خلاصه

با `Arguments = new object[] { ... }` در `[TypeFilter]` می‌توانید پارامترهای سازنده فیلتر را در محل استفاده تعیین کنید. فیلتر `ResponseHeaderActionFilter` الگوی «فیلتر قابل پیکربندی» را نشان می‌دهد: یک کلاس، ده‌ها پیکربندی. این الگو پایه پروژه‌های بعدی  Global Filters و Order  است.
