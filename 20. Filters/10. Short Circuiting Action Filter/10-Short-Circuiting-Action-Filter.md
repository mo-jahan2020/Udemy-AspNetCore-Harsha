# پروژه ۱۰ — Short Circuiting Action Filter (قطع زنجیره در فیلتر)

> منبع: پوشه `20. Filters/10. Short Circuiting Action Filter`
> پیش‌نیاز: پروژه ۰۹ (فیلتر Async و مفهوم `await next()`)

---

## 🎯 مبحث این پروژه چیست؟

 Short-Circuiting  یعنی: فیلتر در میانه راه تصمیم بگیرد «بقیه زنجیره اجرا نشود». در این پروژه یک فیلتر کاربردی و واقعی می‌سازیم 
  `PersonCreateAndEditPostActionFilter`  که هنگام Submit فرم Create/Edit افراد، اگر 
  `ModelState` نامعتبر باشد،  بدون اجرای Action  مستقیماً همان View فرم را با خطاها برمی‌گرداند.

کلید این قابلیت در فیلتر Async ساده است:  اگر 
`next()` را صدا نزنیم، هیچ‌چیز بعد از فیلتر اجرا نمی‌شود  
نه فیلترهای بعدی، نه خود Action Method.
فقط باید `context.Result` را تنظیم کنیم تا Response مشخص باشد.

## 💡 چرا به این مبحث نیاز داریم؟

پیش از این فیلتر، هر دو Action یعنی `Create` و `Edit (POST)` بخش تکراری زیر را داشتند:

```csharp
if (!ModelState.IsValid)
{
    ViewBag.Countries = ...;   // پر کردن مجدد لیست کشورها
    ViewBag.Errors = ...;      // جمع کردن خطاها
    return View(personRequest);
}
```

این منطق مشترک، کاندیدای ایده‌آل یک فیلتر است. با Short-Circuiting:

- اعتبارسنجی و بازگرداندن View خطا از  هر دو  Action حذف و به یک فیلتر منتقل می‌شود؛
- Action Method فقط مسیر «موفق» را می‌بیند و کدش تمیز می‌ماند؛
- درخواست‌های نامعتبر هرگز به لایه Service/DB نمی‌رسند.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/PersonCreateAndEditPostActionFilter.cs`

```csharp
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
 public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
 {
  private readonly ICountriesService _countriesService;   // سرویس DI داخل فیلتر

  public PersonCreateAndEditPostActionFilter(ICountriesService countriesService)
  {
   _countriesService = countriesService;
  }

  public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                           ActionExecutionDelegate next)
  {
   if (context.Controller is PersonsController personsController)   // Cast امن
   {
    if (!personsController.ModelState.IsValid)
    {
     // ۱) پر کردن مجدد داده‌های فرم (لیست کشورها)
     List<CountryResponse> countries = await _countriesService.GetAllCountries();
     personsController.ViewBag.Countries = countries.Select(temp =>
       new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

     // ۲) جمع‌آوری پیام‌های خطا برای نمایش
     personsController.ViewBag.Errors = personsController.ModelState.Values
         .SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

     // ۳) تنظیم Result و قطع زنجیره — Action اجرا نمی‌شود!
     var personRequest = context.ActionArguments["personRequest"];
     context.Result = personsController.View(personRequest);
    }
    else
    {
     await next();   // معتبر است → ادامه بده
    }
   }
   else
   {
    await next();   // روی Controller دیگر هستیم → دخالت نکن
   }
  }
 }
}
```

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[action]")]
[HttpPost]
[TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]   // روی Create POST
public async Task<IActionResult> Create(PersonAddRequest personRequest) { ... }

[Route("[action]/{personID}")]
[HttpPost]
[TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]   // و روی Edit POST
public async Task<IActionResult> Edit(PersonUpdateRequest personRequest) { ... }
```

## ⚙️ مکانیزم Short-Circuiting

```text
Request (فرم نامعتبر)
  → فیلتر: ModelState نامعتبر؟
      ├── بله  → context.Result = View(...) ;  بدون next()  ← زنجیره قطع
      │            (فیلترهای بعدیِ Action و خود Action اجرا نمی‌شوند)
      └── خیر  → await next() → Action اجرا می‌شود
```

نکته: `context.Result` از نوع `IActionResult` است؛ وقتی مقدار بگیرد، ASP.NET Core آن را به‌عنوان «نتیجه نهایی» می‌پذیرد و به مرحله اجرای Result (رندر View) می‌رود.

## 🧪 رفتار برنامه در اجرا

- فرم Create را با داده نامعتبر (مثلاً ایمیل تکراری یا فیلد خالی) submit کنید: فوراً همان فرم با پیام‌های خطا و لیست کشورهای پُرشده برمی‌گردد — بدون آن‌که حتی یک خط از بدنه Action اجرا شود (لاگ داخل Action چاپ نمی‌شود).
- داده معتبر: `await next()` اجرا و Action ادامه می‌یابد.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `context.Result = ...` | تعیین نتیجه نهایی و قطع بقیه Pipeline فیلترها + Action |
| عدم فراخوانی `next()` | نسخه Async از قطع زنجیره (معادل تنظیم Result در نسخه Sync) |
| `context.Controller is PersonsController` | Cast امن با Pattern Matching |
| `ModelState.IsValid` | بررسی معتبر بودن داده ورودی (DataAnnotations) |
| `context.ActionArguments["personRequest"]` | گرفتن مدل submit شده برای پاس دادن به View |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  حین Short-Circuit هم Result Filterها معمولاً اجرا می‌شوند:  قطع کردن در Action Filter یعنی «فیلترهای بعدیِ هم‌نوع و Action اجرا نشوند»، اما مرحله Result (رندر نتیجه) انجام می‌شود — View شما رندر می‌شود. برای کنترل این مرحله، پروژه‌های ۱۱ و ۱۵ و ۱۶ را ببینید.
2.  ViewData فراموش‌شده = فرم خراب:  چون View فرم به `ViewBag.Countries` نیاز دارد، اگر فیلتر آن را پر نکند، صفحه خطا می‌دهد. این دقیقاً کاری است که این فیلتر انجام می‌دهد.
3.  Cast امن را رعایت کنید:  `is PersonsController` تضمین می‌کند فیلتر روی Controller دیگر Crash نکند.
4.  Short-Circuit عمدی vs ناخواسته:  در پروژه ۰۹ دیدیم که فراموشی `next()` خطاست؛ اینجا همان تکنیک را «عمدی و همراه با Result» استفاده کرده‌ایم. همیشه وقتی next را صدا نمی‌زنید، Result تنظیم کنید.

## ✅ خلاصه

Short-Circuiting یعنی تنظیم `context.Result` و فراخوانی نکردن `next()` — فیلترهای بعدی و خود Action نادیده گرفته می‌شوند. فیلتر `PersonCreateAndEditPostActionFilter` الگوی واقعی «اعتبارسنجی مشترک فرم‌های Create/Edit» را پیاده کرد: خطاها و داده‌های فرم در فیلتر آماده و View مستقیم برمی‌گردد. این فیلتر تا انتهای فصل در پروژه‌ها استفاده و در پروژه ۱۵ اثر قطع آن روی انواع دیگر فیلترها بررسی می‌شود.
