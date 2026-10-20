# فهرست فایل‌های آموزشی فصل «Filters» — دوره ASP.NET Core (Harsha)

این مجموعه شامل ۲۳ فایل آموزشی است که هر کدام، **مبحث اصلی** یکی از ۲۳ زیرپروژه پوشه `20. Filters` را توضیح می‌دهد.
هر فایل مستقل است: تعریف مبحث → چرایی → کد نمونه از خود پروژه → نحوه ثبت/استفاده → جدول APIها → نکات کلیدی.

| # | فایل | مبحث |
|---|------|------|
| ۰۱ | [01-Action-Filter.md](01-Action-Filter.md) | ساخت اولین Action Filter با `IActionFilter` |
| ۰۲ | [02-Parameter-Validation-in-Action-Filter.md](02-Parameter-Validation-in-Action-Filter.md) | اعتبارسنجی و اصلاح پارامترها با `ActionArguments` |
| ۰۳ | [03-ViewData-in-Action-Filter.md](03-ViewData-in-Action-Filter.md) | دسترسی به Controller و پر کردن ViewData/ViewBag از فیلتر |
| ۰۴ | [04-Serilog-Structured-Logging.md](04-Serilog-Structured-Logging.md) | لاگ ساختاریافته با Template و Placeholder |
| ۰۵ | [05-Filter-Arguments.md](05-Filter-Arguments.md) | ارسال آرگومان به فیلتر با `TypeFilter(Arguments=...)` |
| ۰۶ | [06-Global-Filters.md](06-Global-Filters.md) | ثبت فیلتر سراسری روی کل برنامه |
| ۰۷ | [07-Custom-Order-of-Filters.md](07-Custom-Order-of-Filters.md) | کنترل ترتیب اجرا با `Order` در Attribute |
| ۰۸ | [08-IOrderedFilter.md](08-IOrderedFilter.md) | پیاده‌سازی `IOrderedFilter` داخل خود کلاس فیلتر |
| ۰۹ | [09-Async-Filters.md](09-Async-Filters.md) | فیلترهای Async با `IAsyncActionFilter` و `await next()` |
| ۱۰ | [10-Short-Circuiting-Action-Filter.md](10-Short-Circuiting-Action-Filter.md) | قطع زنجیره با `context.Result` (فیلتر اعتبارسنجی فرم) |
| ۱۱ | [11-Result-Filter.md](11-Result-Filter.md) | فیلتر نتیجه با `IAsyncResultFilter` |
| ۱۲ | [12-Resource-Filter.md](12-Resource-Filter.md) | فیلتر منبع با `IAsyncResourceFilter` (Feature Flag) |
| ۱۳ | [13-Authorization-Filter.md](13-Authorization-Filter.md) | فیلتر مجوزدهی با `IAuthorizationFilter` (چک کوکی) |
| ۱۴ | [14-Exception-Filter.md](14-Exception-Filter.md) | مدیریت استثناها با `IExceptionFilter` |
| ۱۵ | [15-Impact-of-Short-Circuiting.md](15-Impact-of-Short-Circuiting.md) | اثر قطع زنجیره روی Pipeline کامل فیلترها |
| ۱۶ | [16-IAlwaysRunResultFilter.md](16-IAlwaysRunResultFilter.md) | فیلتر نتیجه همیشه‌اجرا |
| ۱۷ | [17-Filter-Overrides.md](17-Filter-Overrides.md) | ساخت `[SkipFilter]` سفارشی و `context.Filters` |
| ۱۸ | [18-Service-Filter.md](18-Service-Filter.md) | واکشی فیلتر از DI با `[ServiceFilter]` |
| ۱۹ | [19-Filter-Attribute-Classes.md](19-Filter-Attribute-Classes.md) | تبدیل فیلتر به Attribute با `ActionFilterAttribute` |
| ۲۰ | [20-IFilterFactory.md](20-IFilterFactory.md) | کارخانه فیلتر — ترکیب Attribute + DI |
| ۲۱ | [21-UI-Enhancements-Part-1.md](21-UI-Enhancements-Part-1.md) | ارتقای UI بخش اول (Layout، جستجو، مرتب‌سازی) |
| ۲۲ | [22-UI-Enhancements-Part-2.md](22-UI-Enhancements-Part-2.md) | ارتقای UI بخش دوم (Breadcrumb در همه صفحات + پیش‌فرض‌دهی فیلتر) |
| ۲۳ | [23-Configure-Services-Extension.md](23-Configure-Services-Extension.md) | سازماندهی Program.cs با Extension Method |

## نقشه ذهنی Pipeline فیلترها (جمع‌بندی سریع)

```text
Request
 → Authorization Filter   (۱۳)
 → Resource Filter        (۱۲)
 → Action Filter: before  (۰۱–۱۰)  ← ترتیب با Order (۰۷/۰۸)
 → Action Method
 → Action Filter: after
 → Result Filter          (۱۱) — نسخه همیشه‌اجرا: IAlwaysRunResultFilter (۱۶)
 → اجرای نتیجه (رندر View)
 → Resource Filter: after
 → Exception Filter       (۱۴) — فقط اگر استثنا رخ دهد
Response
```

## راه‌های ساخت/اتصال فیلتر (مقایسه سریع)

| روش | DI | آرگومان | سینتکس |
|---|---|---|---|
| `[TypeFilter(typeof(T), Arguments=...)]` | ✅ | ✅ | بلند (پروژه ۰۵) |
| `[ServiceFilter(typeof(T))]` | ✅ کامل | ❌ | متوسط (پروژه ۱۸) |
| `[T(args)]` — ارث‌بری از `ActionFilterAttribute` | ❌ | ✅ | کوتاه (پروژه ۱۹) |
| `IFilterFactory` | ✅ | ✅ | کوتاه + کامل (پروژه ۲۰) ✨ |
