# پروژه ۱۸ — Service Filter (فیلتر از DI Container)

> منبع: پوشه `20. Filters/18. Service Filter`
> پیش‌نیاز: پروژه‌های ۰۵ و ۰۶ (TypeFilter و آرگومان‌ها) و فصل Dependency Injection

---

## 🎯 مبحث این پروژه چیست؟

تا حالا دو راه برای ساخت نمونه فیلتر دیدیم: `TypeFilter` (ساخت با ترکیب DI + آرگومان‌های دستی). این پروژه راه سوم و تمیزتر را معرفی می‌کند: **`ServiceFilter`** — فیلتری که **تماماً از DI Container** حل (resolve) می‌شود.

دو تغییر در کد لازم است:

**۱) ثبت فیلتر در DI (مثل هر سرویس دیگر):**

فایل: `CRUDExample/Program.cs`

```csharp
builder.Services.AddTransient<PersonsListActionFilter>();   // ← فیلتر به‌عنوان سرویس
```

**۲) اتصال با `ServiceFilter` به‌جای `TypeFilter`:**

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[action]")]
[Route("/")]
[ServiceFilter(typeof(PersonsListActionFilter), Order = 4)]   // ← تغییریافته
[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { ..., 1 }, Order = 1)]
[TypeFilter(typeof(PersonsListResultFilter))]
[SkipFilter]
public async Task<IActionResult> Index(...) { ... }
```

## 💡 چرا به این مبحث نیاز داریم؟

`TypeFilter` هر بار که لازم شود، نمونه فیلتر را با `ActivatorUtilities` می‌سازد؛ فیلتر در DI **ثبت نیست**. `ServiceFilter` برعکس:

| جنبه | `[TypeFilter]` | `[ServiceFilter]` |
|---|---|---|
| نمونه‌سازی | درجا ساخته می‌شود | از **DI Container** گرفته می‌شود |
| نیاز به ثبت در DI | ❌ لازم ندارد | ✅ **اجباری** (وگرنه Exception) |
| آرگومان دستی سازنده | ✅ با `Arguments` | ❌ ممکن نیست — همه‌چیز باید از DI بیاید |
| Lifetime | هر بار جدید (به‌جز پیکربندی خاص) | طبق Lifetime ثبت‌شده (Transient/Scoped/Singleton) |
| کاربرد | فیلتر با پیکربندی محلی | فیلتر با وابستگی‌های سنگین DI |

مزیت کلیدی ServiceFilter: فیلتر می‌تواند **هر تعداد سرویس** را در سازنده بگیرد و شما فقط ثبتش می‌کنید؛ و Lifetime آن کاملاً قابل کنترل است.

## 🔍 فیلتر مورد استفاده

`PersonsListActionFilter` از پروژه‌های قبل تغییری نکرده است (ILogger و منطق ViewData). تنها نکته جدید این است که حالا **خودِ کلاس فیلتر، یک سرویس ثبت‌شده** است:

```csharp
public class PersonsListActionFilter : IActionFilter
{
  private readonly ILogger<PersonsListActionFilter> _logger;

  public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
  {
   _logger = logger;
  }
  // OnActionExecuting / OnActionExecuted مثل قبل
}
```

## ⚙️ جریان کار ServiceFilter

```text
Request به Index می‌رسد
 → [ServiceFilter(typeof(PersonsListActionFilter))]
 → ASP.NET Core از IServiceProvider می‌پرسد: PersonsListActionFilter را بده
 → DI Container نمونه می‌سازد (Transient → هر Request یک نمونه) و ILogger را تزریق می‌کند
 → فیلتر اجرا می‌شود
```

## 🧪 رفتار برنامه در اجرا

رفتار بیرونی دقیقاً مثل قبل است (لاگ، اصلاح searchBy، ست ViewData). تفاوت فقط در **منبع نمونه‌سازی** است:

- اگر ثبت `AddTransient<PersonsListActionFilter>()` را حذف کنید، این Exception را می‌گیرید:

```text
InvalidOperationException: No service for type
'CRUDExample.Filters.ActionFilters.PersonsListActionFilter' has been registered.
```

- اگر Lifetime را `AddScoped` کنید، در هر Request یک نمونه (و در یک Request چند محل استفاده، همان نمونه) خواهید داشت.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `[ServiceFilter(typeof(T), Order = n)]` | واکشی فیلتر از DI Container |
| `builder.Services.AddTransient<T>()` | ثبت فیلتر به‌عنوان سرویس |
| Lifetime (Transient/Scoped/Singleton) | کنترل تعداد و عمر نمونه‌های فیلتر |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **فراموشی ثبت = Exception در زمان اجرا:** رایج‌ترین خطای این پروژه همان است که بالا دیدید؛ اگر با `ServiceFilter` خطای "No service for type..." گرفتید، اول سراغ Program.cs بروید.
2. **با ServiceFilter آرگومان دستی پاس نمی‌دهید:** سازنده فیلتر فقط سرویس‌ها را می‌گیرد؛ اگر به «مقدار پیکربندی» نیاز دارید یا آن را از Configuration تزریق کنید یا سراغ پروژه ۲۰ (IFilterFactory) بروید.
3. **احتیاط با Singleton:** فیلتر Singleton نمی‌تواند سرویس Scoped (مثل DbContext) را مستقیم تزریق کند (Captive Dependency)؛ Transient/Scoped امن‌ترین انتخاب‌ها برای فیلترهاست.
4. **`Order` هنوز کار می‌کند:** همان قواعد پروژه‌های ۰۷/۰۸ روی ServiceFilter هم برقرار است.

## ✅ خلاصه

`ServiceFilter` فیلتر را به یک سرویس تمام‌عیار تبدیل می‌کند: ثبت در DI با `AddTransient/Scoped/Singleton`، واکشی خودکار وابستگی‌ها و مدیریت Lifetime. با این پروژه سه‌گانه «TypeFilter / ServiceFilter / (به‌زودی) FilterFactory» کامل می‌شود و در پروژه بعد روش چهارم — تبدیل خود فیلتر به Attribute — معرفی می‌شود.
