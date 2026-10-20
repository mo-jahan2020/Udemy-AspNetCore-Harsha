# پروژه ۰۶ — Global Filters (فیلترهای سراسری)

> منبع: پوشه `20. Filters/06. Global Filters`
> پیش‌نیاز: پروژه ۰۵ (آرگومان‌های فیلتر)

---

## 🎯 مبحث این پروژه چیست؟

تا حالا فیلترها را با Attribute روی Action یا Controller وصل می‌کردیم.
در این پروژه یاد می‌گیریم فیلتر را به‌صورت  Global  (سراسری) ثبت کنیم تا  روی تمام 
Action های کل برنامه  اجرا شود — بدون آن‌که حتی یک Attribute بنویسیم.

محل ثبت سراسری، همان‌جایی است که MVC پیکربندی می‌شود:

```csharp
builder.Services.AddControllersWithViews(options => {
    options.Filters.Add(...);
});
```

## 💡 چرا به این مبحث نیاز داریم؟

برخی نگرانی‌ها ذاتاً «برنامه‌ای» هستند، نه مخصوص یک Action: لاگ کلی، مدیریت سراسری خطا (پروژه ۱۴)، اضافه کردن Header های امنیتی به همه Response ها و... نوشتن Attribute برای تک‌تک Actionها هم کد تکراری می‌سازد و هم در فراموشی یک Action، حفره امنیتی/رفتاری ایجاد می‌کند. فیلتر Global این تضمین را می‌دهد:  هیچ Action از قلم نیفتد. 

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Program.cs` (بخش تغییر یافته)

```csharp
using CRUDExample.Filters.ActionFilters;

//it adds controllers and views as services
builder.Services.AddControllersWithViews(options => {
 //options.Filters.Add<ResponseHeaderActionFilter>();   // روش Generic (فیلتر بدون آرگومان)

 var logger = builder.Services.BuildServiceProvider()
                    .GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

 // روش Instance: نمونه آماده با آرگومان‌ها
 options.Filters.Add(new ResponseHeaderActionFilter(logger, "My-Key-From-Global", "My-Value-From-Global"));
});
```

و در Controller، فیلتر قبلی همچنان در سطح‌های مختلف باقی است تا مقایسه ممکن شود:

```csharp
[Route("[controller]")]
[TypeFilter(typeof(ResponseHeaderActionFilter),
    Arguments = new object[] { "My-Key-From-Controller", "My-Value-From-Controller" })] // سطح Controller
public class PersonsController : Controller { ... }

[TypeFilter(typeof(ResponseHeaderActionFilter),
    Arguments = new object[] { "MyKey-FromAction", "MyValue-From-Action" })]             // سطح Action
public async Task<IActionResult> Index(...) { ... }
```

## ⚙️ سه روش افزودن فیلتر Global

| روش | کد | کاربرد |
|---|---|---|
|  Type  | `options.Filters.Add<ResponseHeaderActionFilter>();` | وقتی فیلتر سازنده بدون آرگومان یا فقط سرویس‌های DI دارد |
|  Instance  | `options.Filters.Add(new ResponseHeaderActionFilter(logger, "K", "V"));` | وقتی می‌خواهید آرگومان‌ها را همان‌جا پاس دهید |
|  Type با Factory  | `options.Filters.AddService<T>()` (پروژه ۱۸) | وقتی فیلتر از DI حل شود |

## 🧭 ترتیب اجرای سه سطح

وقتی یک Action هم فیلتر Global دارد، هم Controller و هم Action، ترتیب اجرای «Before» ها چنین است:

```text
Global  →  Controller  →  Action  →  خود Action Method
   (بعد از اجرا، "After" ها به ترتیب معکوس برمی‌گردند)
```

یعنی در این پروژه برای `Index` سه هدر مختلف در Response خواهید داشت و در لاگ سه‌بار Before/After دیده می‌شود.

## 🧪 رفتار برنامه در اجرا

هر Response (حتی صفحاتی که هیچ Attribute فیلتری ندارند) این Header را دارد:

```text
My-Key-From-Global: My-Value-From-Global
```

## ⚠️ نکات کلیدی و خطاهای رایج

1.  هشدار `BuildServiceProvider`:  خط `builder.Services.BuildServiceProvider().GetRequiredService<ILogger<...>>()` برای گرفتن لاگر قبل از Build کار می‌کند اما یک anti-pattern شناخته‌شده است (ServiceProvider دومی می‌سازد). در ادامه دوره راه‌حل تمیزتر (ServiceFilter / FilterFactory) ارائه می‌شود؛ اینجا صرفاً برای نمایش مفهوم است.
2.  Global یعنی همه جا — دقت کنید:  این فیلتر روی حتی صفحات خطا و Actionهای ادمین هم اجرا می‌شود. قبل از Global کردن، اطمینان پیدا کنید رفتار فیلتر برای همه Actionها بی‌ضرر است.
3.  ترتیب Before/After:  همه مرحلههای Before به ترتیب Global→Controller→Action اجرا می‌شوند؛ مرحله‌های After دقیقاً برعکس. (کنترل دقیق‌تر این ترتیب در پروژه‌های ۰۷ و ۰۸ با Order.)
4.  Instance مشترک می‌ماند:  فیلتری که به‌صورت Instance به Global اضافه کنید، یک نمونه است که برای همه Requestها استفاده می‌شود؛ فیلتر خود را Thread-Safe و Stateless بنویسید.

## ✅ خلاصه

فیلتر سراسری با `AddControllersWithViews(options => options.Filters.Add(...))` ثبت می‌شود و روی همه Actionها اجرا می‌شود. سه سطح اعمال فیلتر (Global/Controller/Action) مکمل هم‌اند و در Pipeline به ترتیب خارج به داخل اجرا می‌شوند. این پروژه همچنین ضعف «ساخت نمونه دستی با آرگومان» را نشان می‌دهد که انگیزه پروژه‌های بعدی است.
