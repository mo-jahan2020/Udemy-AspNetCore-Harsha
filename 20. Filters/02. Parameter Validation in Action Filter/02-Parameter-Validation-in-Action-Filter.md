# پروژه ۰۲ — Parameter Validation in Action Filter (اعتبارسنجی پارامترها در فیلتر)

> منبع: پوشه `20. Filters/02. Parameter Validation in Action Filter`
> پیش‌نیاز: پروژه ۰۱ (ساخت اولین Action Filter)

---

## 🎯 مبحث این پروژه چیست؟

در این پروژه می‌آموزیم که چگونه از داخل Action Filter به پارامترهای ورودی 
Action Method دسترسی پیدا کنیم، آن‌ها را اعتبارسنجی
کنیم و در صورت نیاز مقدارشان را تغییر دهیم  پیش از آن‌که Action اجرا شود.

ابزار اصلی این کار، پراپرتی`context.ActionArguments` از نوع `ActionExecutingContext` 
است؛ یک Dictionary که کلید آن نام پارامتر و مقدار آن، مقدار Model-Binding شده همان پارامتر است.

## 💡 چرا به این مبحث نیاز داریم؟

در پروژه قبلی، فیلتر فقط لاگ می‌نوشت و به داده‌های ورودی کاری نداشت.
اما در دنیای واقعی یکی از پرکاربردترین جایگاه‌های Action Filter
دقیقاً همین بازرسی و پاک‌سازی ورودی است. سناریوی پروژه:
کاربر می‌تواند با پارامتر `searchBy` مشخص کند جستجو روی کدام ستون انجام شود
PersonName، Email و.... اگر کاربر عمداً یا سهواً مقدار نامعتبری مثل
`searchBy=xyz` بفرستد، بهتر است فیلتر قبل از رسیدن به Action و لایه سرویس، آن را به مقدار پیش‌فرض امن
`PersonName` اصلاح کند.
این کار:

- کد Action را از منطق اعتبارسنجی ورودیِ تکراری خالی می‌کند؛
- از رسیدن مقادیر خراب به لایه Service/Repository جلوگیری می‌کند؛
- یک نقطه مرکزی برای «قواعد ورودی» ایجاد می‌کند.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/PersonsListActionFilter.cs` (متد OnActionExecuting)

```csharp
public void OnActionExecuting(ActionExecutingContext context)
{
 _logger.LogInformation("PersonsListActionFilter.OnActionExecuting method");

 if (context.ActionArguments.ContainsKey("searchBy"))   // آیا پارامتر searchBy وجود دارد؟
 {
  string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);

  // اعتبارسنجی مقدار پارامتر searchBy
  if (!string.IsNullOrEmpty(searchBy))
  {
   var searchByOptions = new List<string>() {
    nameof(PersonResponse.PersonName),
    nameof(PersonResponse.Email),
    nameof(PersonResponse.DateOfBirth),
    nameof(PersonResponse.Gender),
    nameof(PersonResponse.CountryID),
    nameof(PersonResponse.Address)
   };

   // اگر مقدار مجاز نبود، مقدار پارامتر بازنویسی می‌شود
   if (searchByOptions.Any(temp => temp == searchBy) == false)
   {
    _logger.LogInformation("searchBy actual value {searchBy}", searchBy);
    context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
    _logger.LogInformation("searchBy updated value {searchBy}", context.ActionArguments["searchBy"]);
   }
  }
 }
}
```

## ⚙️ نحوه کار

1. Model Binding پارامترهای `Index` (یعنی `searchBy`، `searchString`، `sortBy`، `sortOrder`) را از Query String پر می‌کند.
2. فیلتر در متد `OnActionExecuting` اجرا می‌شود — هنوز Action اجرا نشده است.
3. فیلتر چک می‌کند آیا کلید `"searchBy"` در `context.ActionArguments` هست.
4. اگر مقدار آن یکی از فیلدهای مجازِ `PersonResponse` نباشد، فیلتر همان سلول Dictionary را با `nameof(PersonResponse.PersonName)` **بازنویسی** می‌کند.
5. سپس Action اجرا می‌شود و پارامتر `searchBy` که دریافت می‌کند، همان مقدار اصلاح‌شده است — نه مقدار اصلی کاربر.

## 🧪 رفتار برنامه در اجرا

| درخواست کاربر | مقدار پس از فیلتر | نتیجه |
|---|---|---|
| `?searchBy=Email&searchString=a` | `Email` | جستجو روی ایمیل (دست‌نخورده) |
| `?searchBy=xyz&searchString=a` | `PersonName` | اصلاح به ستون پیش‌فرض و ادامه جستجو |
| بدون `searchBy` | `null` | فیلتر کاری نمی‌کند؛ Action با مقدار پیش‌فرض می‌رود |

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `ActionExecutingContext.ActionArguments` | Dictionary پارامترهای Action؛ خواندنی و **نوشتنی** |
| `ContainsKey("searchBy")` | بررسی وجود پارامتر قبل از خواندن (جلوگیری از Exception) |
| `nameof(PersonResponse.PersonName)` | تولید امنِ نام فیلد به‌صورت رشته، مقاوم در برابر Rename |

## ⚠️ نکات کلیدی و خطاهای رایج

1. **تغییر پارامتر دائمی است برای همان Request:** مقدار بازنویسی‌شده به Action، لاگرها و هر فیلتر بعدی منتقل می‌شود، اما URL کاربر تغییر نمی‌کند.
2. **نام پارامتر Case-Sensitive نیست اما دقیق است:** کلید Dictionary همان نام پارامتر در امضای Action است؛ اگر نام را در Action تغییر دهید، کلید فیلتر هم باید تغییر کند (بهتر است با `nameof` بسازید).
3. **این اعتبارسنجی جای DataAnnotations را نمی‌گیرد:** این الگو برای «نرمال‌سازی و اصلاح» ورودی است؛ برای اعتبارسنجی Model با Data Annotations و `ModelState` از پروژه ۱۰ استفاده می‌کنیم.
4. **خواندن بدون چک = خطر:** اگر بدون `ContainsKey` مستقیماً `context.ActionArguments["searchBy"]` را بخوانید و پارامتر در Action وجود نداشته باشد، `KeyNotFoundException` می‌گیرید.

## ✅ خلاصه

`ActionArguments` پلی است بین فیلتر و پارامترهای Action. در این پروژه فیلترِ لیست افراد، مقدار `searchBy` را قبل از اجرای Action اعتبارسنجی و در صورت نیاز بازنویسی می‌کند. این الگوی «بازرسی و اصلاح ورودی در فیلتر» یکی از رایج‌ترین کاربردهای Action Filter در پروژه‌های واقعی است.
