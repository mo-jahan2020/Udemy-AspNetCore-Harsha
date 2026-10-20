# پروژه ۱۳ — Authorization Filter (فیلتر مجوزدهی)

> منبع: پوشه `20. Filters/13. Authorization Filter`
> پیش‌نیاز: پروژه ۱۲ (آشنایی با ترتیب Pipeline فیلترها)

---

## 🎯 مبحث این پروژه چیست؟

**Authorization Filter** اولین فیلتر در کل Pipeline فیلترهاست — حتی قبل از Resource Filter. وظیفه‌اش یک چیز است: تصمیم‌گیری که «آیا اصلاً اجازه ادامه دهیم یا نه؟». اینترفیس آن **`IAuthorizationFilter`** است با تنها یک متد:

```csharp
public void OnAuthorization(AuthorizationFilterContext context)
{
    // تصمیم: ادامه بده یا context.Result = ... بگذار و قطع کن
}
```

در این پروژه فیلتر `TokenAuthorizationFilter` ساخته می‌شود که ورود کاربر را با یک **Cookie** بررسی می‌کند: اگر کوکی `Auth-Key` با مقدار `A100` وجود نداشته باشد، پاسخ **401 Unauthorized** داده می‌شود.

## 💡 چرا به این مبحث نیاز داریم؟

اعتبارسنجی/مجوزدهی باید «سریع و اول» انجام شود؛ بی‌معناست که دیتابیس بخوانیم و Model Bind کنیم و آخرش بگوییم «دسترسی نداری». Authorization Filter دقیقاً برای همین در ابتدای Pipeline است:

- امن‌ترین و ارزان‌ترین نقطه برای قطع درخواست‌های غیرمجاز؛
- پایه‌ای که در فصل‌های بعدی (Identity و JWT) با نسخه‌های استاندارد آن (`[Authorize]`) جایگزین می‌شود؛
- آموزش مفهوم «اولین نقطه تصمیم» که در طراحی APIها حیاتی است.

> ⚠️ در پروژه‌های واقعی، Security-sensitive احراز هویت را با فیلتر دستی ننویسید؛ از سیستم Authentication/Authorization خود ASP.NET Core استفاده کنید. این پروژه آموزشِ مکانیزم است.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/AuthorizationFilter/TokenAuthorizationFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Mime;

namespace CRUDExample.Filters.AuthorizationFilter
{
 public class TokenAuthorizationFilter : IAuthorizationFilter
 {
  public void OnAuthorization(AuthorizationFilterContext context)
  {
   // آیا کوکی Auth-Key اصلاً فرستاده شده؟
   if (context.HttpContext.Request.Cookies.ContainsKey("Auth-Key") == false)
   {
    context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
    return;   // ادامه نده
   }

   // مقدار کوکی درست است؟
   if (context.HttpContext.Request.Cookies["Auth-Key"] != "A100")
   {
    context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
   }
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
[TypeFilter(typeof(TokenAuthorizationFilter))]    // ← فقط روی Edit POST
public async Task<IActionResult> Edit(PersonUpdateRequest personRequest) { ... }
```

اما کوکی `Auth-Key` از کجا می‌آید؟ در همین پروژه، فیلتر `TokenResultFilter` (نوع Result Filter) آن را ست می‌کند — نتیجه بازدید یک‌بار از صفحه `Create` (که `[TypeFilter(typeof(TokenResultFilter))]` دارد) این است:

```csharp
public void OnResultExecuting(ResultExecutingContext context)
{
 context.HttpContext.Response.Cookies.Append("Auth-Key", "A100");
}
```

یعنی سناریوی دمو: کاربر اول صفحه Create را باز می‌کند (کوکی می‌گیرد)، بعد Edit برایش مجاز می‌شود.

## 🧭 جایگاه در Pipeline

```text
Request
 → Authorization Filter  ✅ اولِ همه
 → Resource Filter
 → Action Filter → Action → Action Filter
 → Result Filter → اجرای نتیجه
Response
```

## 🧪 رفتار برنامه در اجرا

| وضعیت کوکی Auth-Key | نتیجه POST به `/persons/edit/...` |
|---|---|
| ندارد | **401 Unauthorized** — Action اصلاً اجرا نمی‌شود |
| مقدار اشتباه (مثلاً `B200`) | **401 Unauthorized** |
| `A100` | زنجیره ادامه می‌یابد و Edit اجرا می‌شود |

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IAuthorizationFilter` | اینترفیس Authorization Filter (فقط متد OnAuthorization) |
| `AuthorizationFilterContext` | دسترسی به HttpContext و قطع با Result |
| `StatusCodes.Status401Unauthorized` | ثابت عددی 401 به‌جای عدد جادویی |
| `Request.Cookies["Auth-Key"]` | خواندن کوکی درخواست |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **فقط سنکرون است:** برخلاف Action/Result/Resource، برای Authorization Filter نسخه Async جدا ندارد (نسخه‌های Async آن در سیاست‌های Authorization داخلی ASP.NET Core پیاده شده‌اند).
2. **برای قطع، حتماً `context.Result` ست کنید:** اگر Result ست نشود، Pipeline ادامه می‌یابد. `return` بعد از ست‌کردن Result برای جلوگیری از اجرای ادامه کد است.
3. **مقایسه با Resource Filter:** هر دو می‌توانند قطع کنند؛ تفاوت در «زمان». Authorization قبل از Resource است و قرار است فقط تصمیم مجوز بگیرد، نه کار دیگری.
4. **مقدار کوکی در کد هاردکد است:** در پروژه‌های واقعی این مقدار باید از Authentication (توکن امضاشده) بیاید. هدف این‌جا نمایش مکانیزم فیلتر است.

## ✅ خلاصه

Authorization Filter اولین حلقه Pipeline فیلترهاست؛ با `IAuthorizationFilter.OnAuthorization` و ست‌کردن `context.Result` (اینجا: 401) درخواست‌های غیرمجاز را در ارزان‌ترین نقطه قطع می‌کند. فیلتر `TokenAuthorizationFilter` الگوی «چک کوکی/توکن» را آموزش داد و مفهوم کاملِ Pipeline حالا آماده است تا در پروژه ۱۴ با مدیریت استثناها تکمیل شود.
