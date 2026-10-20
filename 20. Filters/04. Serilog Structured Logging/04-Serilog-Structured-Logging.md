# پروژه ۰۴ — Serilog Structured Logging  لاگ ساختاریافته در فیلترها 

> منبع: پوشه `20. Filters/04. Serilog Structured Logging`
> پیش‌نیاز: فصل Logging  پروژه ۱۹ دوره  و پروژه‌های ۰۱ تا ۰۳ فیلترها

---

## 🎯 مبحث این پروژه چیست؟

    در این پروژه روش نوشتن لاگ در فیلترها از رشته‌های ثابت به  
    Structured Logging   لاگ ساختاریافته  ارتقا می‌یابد. لاگ ساختاریافته یعنی به‌جای درج مقادیر داخل رشته با
    interpolation، مقادیر به‌صورت  Template و Placeholder  ارسال شوند
    تا بعنوان فیلدهای جداگانه و قابل جستجو ذخیره شوند.

تغییر اصلی پروژه، بازنویسی خطوط لاگ فیلتر به این شکل است:

```csharp
// قبلاً  پروژه ۰۱ :
_logger.LogInformation "PersonsListActionFilter.OnActionExecuting method" ;

// حالا  پروژه ۰۴ :
_logger.LogInformation "{FilterName}.{MethodName} method",
    nameof PersonsListActionFilter , nameof OnActionExecuting  ;
```

## 💡 چرا به این مبحث نیاز داریم؟

رشته‌های ساده در لاگ، خوانا هستند اما قابل پرس‌وجو نیستند. وقتی هزاران خط لاگ در فایل یا دیتابیس Serilog دارید، می‌خواهید بپرسید: «همه لاگ‌های فیلتر X را نشان بده» یا «فقط OnActionExecuting ها را». با Structured Logging:

- هر `{Placeholder}` یک  فیلد جداگانه  با نام مشخص می‌شود  مثلاً `FilterName` و `MethodName` ؛
- می‌توانید در Sink ها  فایل JSON، Seq، دیتابیس  بر اساس این فیلدها  فیلتر و Query  کنید؛
- نگارش کد تمیزتر می‌شود و از string interpolation `$"..."` که پارس لاگ را غیرممکن می‌کند، دوری می‌کنیم.

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/PersonsListActionFilter.cs`  بخش‌های تغییر یافته 

```csharp
public void OnActionExecuted ActionExecutedContext context 
{
 _logger.LogInformation "{FilterName}.{MethodName} method",
     nameof PersonsListActionFilter , nameof OnActionExecuted  ;

 // ...  منطق ViewData پروژه ۰۳ بدون تغییر 
}

public void OnActionExecuting ActionExecutingContext context 
{
 context.HttpContext.Items["arguments"] = context.ActionArguments;

 _logger.LogInformation "{FilterName}.{MethodName} method",
     nameof PersonsListActionFilter , nameof OnActionExecuting  ;

 // ...
 if  searchByOptions.Any temp => temp == searchBy  == false 
 {
  _logger.LogInformation "searchBy actual value {searchBy}", searchBy ;
  context.ActionArguments["searchBy"] = nameof PersonResponse.PersonName ;
  _logger.LogInformation "searchBy updated value {searchBy}", context.ActionArguments["searchBy"] ;
 }
}
```

تنظیمات Serilog هم در `Program.cs`  مثل فصل Logging  برقرار است:

```csharp
builder.Host.UseSerilog  HostBuilderContext context, IServiceProvider services,
    LoggerConfiguration loggerConfiguration  => {

 loggerConfiguration
 .ReadFrom.Configuration context.Configuration  // خواندن تنظیمات از appsettings.json
 .ReadFrom.Services services ;                  // اتصال سرویس‌ها  مثل ILogger  به Serilog
} ;
```

## ⚙️ قواعد نوشتن لاگ ساختاریافته

| قاعده | مثال درست | مثال غلط |
|---|---|---|
| از Placeholder داخل رشته استفاده کنید | `"{searchBy}"`, `searchBy` | `$"searchBy: {searchBy}"` |
| نام Placeholder معنادار باشد | `"{FilterName}.{MethodName}"` | `"{0}.{1}"` |
| مقدارها به‌صورت آرگومان جداگانه داده شوند | `_logger.LogInformation "{x}", x ` | `_logger.LogInformation "x=" + x ` |
| `nameof  ` برای جلوگیری از رشته هاردکد | `nameof OnActionExecuting ` | `"OnActionExecuting"` |

 خروجی در کنسول Serilog  چیزی شبیه این است:

```text
info: PersonsListActionFilter.OnActionExecuting method
      FilterName=PersonsListActionFilter  MethodName=OnActionExecuting
      searchBy=xyz
```

 دقت کنید `FilterName` و `MethodName` به‌عنوان پراپرتی مجزا چاپ/ذخیره می‌شوند، نه بخشی از متن. 

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `_logger.LogInformation template, args... ` | لاگ با Template ساختاریافته |
| `{Placeholder}` | جای‌گذاری مقدار به‌عنوان فیلد مستقل در لاگ |
| `nameof ... ` | تولید نام کلاس/متد به‌صورت Compile-Time  مقاوم در برابر Rename  |
| `builder.Host.UseSerilog ... ` | جایگزینی سیستم لاگ پیش‌فرض با Serilog |
| `ReadFrom.Configuration / Services` | تنظیمات Serilog از فایل کانفیگ و از DI |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  String interpolation را در لاگ ننویسید:  `$"searchBy: {searchBy}"` مقادیر را قبل از رسیدن به لاگر درون رشته می‌چسباند و ساختار را از بین می‌برد؛ همیشه Template بنویسید.
2.  تنظیم MinimumLevel در appsettings.json:  اگر در `Serilog:MinimumLevel` سطح `Information` فیلتر شده باشد، لاگ‌ها دیده نمی‌شوند؛ برای دیباگ می‌توانید سطح را موقتاً روی `Debug` بگذارید.
3.  Placeholder تکراری با مقادیر متفاوت:  در دو خط لاگ پشت‌سرهم از `{searchBy}` برای مقدار «قبل» و «بعد» استفاده شده؛ در Sink ها هر رخداد فیلد خودش را دارد، اما خوانایی کنسول را در نظر بگیرید و پیام‌ها را متمایز کنید  `actual value` / `updated value` .
4.  این الگو در کل پروژه تکرار می‌شود:  از این جا به بعد تمام فیلترهای دوره  Action، Result، Resource، Authorization، Exception  با همین سبک لاگ ساختاریافته نوشته می‌شوند؛ حفظ کنید.

## ✅ خلاصه

لاگ ساختاریافته یعنی «داده، نه متن». با الگوی `"{PropertyName}", value` هر پیام لاگ به مجموعه‌ای از فیلدهای قابل جستجو تبدیل می‌شود و `nameof  ` نام‌ها را از کامپایلر می‌گیرد. این پروژه یک بهبود کیفیت کد است که از این پس در تمام فیلترهای فصل جاری رعایت می‌شود.
