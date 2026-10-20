# پروژه ۱۷ — Filter Overrides (بازنویسی/معافیت از فیلترها)

> منبع: پوشه `20. Filters/17. Filter Overrides`
> پیش‌نیاز: پروژه ۱۶ (IAlwaysRunResultFilter فعال در سطح Controller)

---

## 🎯 مبحث این پروژه چیست؟

مشکل: `PersonAlwaysRunResultFilter` در سطح Controller است و «همیشه» اجرا می‌شود؛ اما برای یک Action خاص (`Index`) نمی‌خواهیم اجرا شود. راه‌حل دوره: ساختن یک **Attribute سفارشی به‌نام `[SkipFilter]`** و چک‌کردن آن داخل فیلترها — یعنی خودمان مکانیزم «معافیت از فیلتر» را می‌سازیم (پیاده‌سازی دستی مفهومی شبیه `[OverrideXxxFilters]` داخلی ASP.NET Core).

دو بخش دارد:

**۱) Attribute علامت‌گذار (Marker):**

فایل: `CRUDExample/Filters/SkipFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters
{
 // یک Attribute خالی که فقط «علامت» است — هیچ منطقی ندارد
 public class SkipFilter : Attribute, IFilterMetadata
 {
 }
}
```

**۲) احترام به علامت، داخل فیلتر:**

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
   // آیا در زنجیره فیلترهای این درخواست، SkipFilter وجود دارد؟
   if (context.Filters.OfType<SkipFilter>().Any())
   {
    return;   // بله → این فیلتر خودش را معاف می‌کند
   }

   //TO DO: before logic here
  }
 }
}
```

**۳) استفاده:**

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
[TypeFilter(typeof(ResponseHeaderActionFilter), ..., Order = 1)]
[TypeFilter(typeof(PersonsListResultFilter))]
[SkipFilter]                                    // ← معافیت از فیلترهایی که Skip را رعایت می‌کنند
public async Task<IActionResult> Index(...) { ... }
```

## 💡 چرا به این مبحث نیاز داریم؟

وقتی فیلتری در سطح Controller/Global است، همیشه یک Action استثنا وجود دارد. `[SkipFilter]` یک الگوی عمومی و قابل توسعه می‌سازد:

- هر فیلتری که بخواهد، با یک `if` به این Attribute احترام می‌گذارد؛
- کنترل کامل دست شماست: می‌توانید انواع SkipFilter با اسم‌های معنادار بسازید (SkipCaching، SkipAudit و...);‌
- این الگو در واقع همان مفهوم `context.Filters` را آموزش می‌دهد: **لیست تمام فیلترهای اعمال‌شده روی درخواست جاری در دسترس هر فیلتر است.**

## ⚙️ چطور `context.Filters` کار می‌کند؟

`context.Filters` یک `IList<IFilterMetadata>` است که **همه فیلترهای** اعمال‌شده روی این درخواست را دارد (Global + Controller + Action). پس فیلتر می‌تواند بپرسد:

```csharp
context.Filters.OfType<SkipFilter>().Any()   // آیا کسی SkipFilter گذاشته؟
```

`IFilterMetadata` اینترفیسِ «علامت» است: هیچ عضوی ندارد و فقط می‌گوید «من یک فیلترم». به همین دلیل `[SkipFilter]` حتی بدون منطق، داخل این لیست ظاهر می‌شود.

## 🧪 رفتار برنامه در اجرا

| Action | [SkipFilter] دارد؟ | AlwaysRun اجرا می‌شود؟ |
|---|---|---|
| Index | ✅ | ❌ (خودش را معاف می‌کند) |
| Create / Delete / Edit | ❌ | ✅ (مثل پروژه ۱۶) |

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `IFilterMetadata` | اینترفیس علامتِ «من فیلترم» بدون هیچ عضوی |
| `Attribute` | پایه‌ای که کلاس را قابل استفاده به‌صورت `[SkipFilter]` می‌کند |
| `context.Filters` | فهرست همه فیلترهای فعال روی درخواست جاری |
| `.OfType<T>().Any()` | جستجوی نوع مشخص در فهرست فیلترها |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **SkipFilter خودش کاری نمی‌کند:** این Attribute هیچ منطق اجرایی ندارد؛ منطقِ احترام به آن را «فیلترهای دیگر» می‌نویسند. اگر فیلتری آن را چک نکند، رفتارش تغییری نمی‌کند.
2. **مقایسه با `[OverrideActionFilters]` داخلی:** ASP.NET Core اتریبیوت‌های Override رسمی دارد (مثل `OverrideActionFiltersAttribute`) که فیلترهای سطح پایین‌تر را کلاً حذف می‌کنند؛ رویکرد این پروژه دستی و هدفمندتر است و فقط فیلترهای «همکاری‌کننده» را کنترل می‌کند.
3. **اگر می‌خواهید فیلتر دیگری هم Skip را رعایت کند:** همان دو خط `if (context.Filters.OfType<SkipFilter>().Any()) return;` را در آن فیلتر اضافه کنید — الگو تکرارشدنی است (در نسخه‌های بعدی همین پروژه در Repository انجام شده است).
4. **جای اتریبیوت مهم نیست:** چه در سطح Action و چه Controller/Global باشد، `context.Filters` همه را می‌بیند؛ پس `[SkipFilter]` حتی در سطح Controller هم معنا دارد.

## ✅ خلاصه

با یک `Attribute + IFilterMetadata` خالی و چک‌کردن آن از طریق `context.Filters.OfType<SkipFilter>().Any()`، مکانیزم «معافیت اختیاری از فیلتر» ساخته شد. این الگو ترکیبی از دو مفهوم دوره است (سطوح فیلتر + لیست فیلترهای فعال) و پایه‌ای منعطف برای کنترل رفتار فیلترهای سراسری/کنترلری است.
