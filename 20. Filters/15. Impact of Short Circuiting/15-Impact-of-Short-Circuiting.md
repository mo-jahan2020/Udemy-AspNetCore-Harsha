# پروژه ۱۵ — Impact of Short Circuiting (اثر قطع زنجیره بر Pipeline)

> منبع: پوشه `20. Filters/15. Impact of Short Circuiting`
> پیش‌نیاز: هر پنج نوع فیلتر (پروژه‌های ۰۱ تا ۱۴)

---

## 🎯 مبحث این پروژه چیست؟

این پروژه یک  آزمایشگاه  است: همه پنج نوع فیلتر هم‌زمان روی Controller/Action
ها نصب شده‌اند تا با لاگ‌های Serilog ببینیم وقتی یکی از فیلترها زنجیره را قطع می‌کند،
دقیقاً کدام فیلترها اجرا می‌شوند و کدام‌ها نادیده گرفته می‌شوند .
هیچ API جدیدی معرفی نمی‌شود؛ هدف، تسلط کامل بر Pipeline است.

ترکیب فیلترهای فعال در این پروژه:

```csharp
[Route("[controller]")]
[TypeFilter(typeof(ResponseHeaderActionFilter), ..., Order = 3)]   // Action Filter (کنترلر)
[TypeFilter(typeof(HandleExceptionFilter))]                        // Exception Filter
public class PersonsController { ... }

// Index:
[TypeFilter(typeof(PersonsListActionFilter), Order = 4)]           // Action Filter
[TypeFilter(typeof(ResponseHeaderActionFilter), ..., Order = 1)]   // Action Filter
[TypeFilter(typeof(PersonsListResultFilter))]                      // Result Filter

// Create POST:
[TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]          // Action Filter (قطع‌کننده!)
[TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[] { false })] // Resource

// Create GET:
[TypeFilter(typeof(TokenResultFilter))]                            // Result Filter (ست کوکی)

// Edit POST:
[TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]          // Action Filter (قطع‌کننده!)
[TypeFilter(typeof(TokenAuthorizationFilter))]                     // Authorization
```

## 💡 چرا به این مبحث نیاز داریم؟

تا وقتی رفتار Pipeline را با چشم نبینید،debug کردن فیلترها سخت است. سؤالاتی که این پروژه پاسخ می‌دهد:

- وقتی Action Filter قطع می‌کند، Result Filter ها اجرا می‌شوند؟ (بله)
- وقتی Authorization قطع می‌کند، چه چیزی اجرا می‌شود؟ (هیچ‌کدام از فیلترهای بعدی)
- ترتیب «قبل» و «بعد» فیلترهای Async چگونه با هم می‌آمیزند؟

## 🧭 جدول مرجع: اثر Short-Circuiting در هر سطح

| کجا قطع می‌کنیم؟ | چه چیزهایی اجرا نمی‌شوند؟ | چه چیزهایی اجرا می‌شوند؟ |
|---|---|---|
|  Authorization Filter  | همه فیلترها + Action + رندر | فقط Middlewareها و خود Authorization |
|  Resource Filter  | Action/Result Filterها + Action + رندر | Authorization + Resource (قبل) و After آن |
|  Action Filter  | فیلترهای Action بعدی + خود Action | Authorization، Resource، و  Result Filterها + رندر  (چون Result ست شده اجرا می‌شود) |
|  Result Filter  (قبل از next) | اجرای نتیجه (رندر View) | همه فیلترهای قبل + Result Filterهای قبل از قطع |
|  Exception Filter  (با Result) | ادامه Pipeline عادی | پاسخ جایگزین تولید می‌شود |

## 🧪 آزمایش‌های کلیدی این پروژه

 ۱) قطع توسط `PersonCreateAndEditPostActionFilter` (ModelState نامعتبر): 

```text
→ Action Filter اجرا می‌شود و Result می‌سازد (View فرم)
✓ Result Filter (اگر روی همان Action بود) اجرا می‌شود → چون مرحله «اجرای نتیجه» رخ می‌دهد
✗ خود Action اجرا نمی‌شود (لاگ داخل Action چاپ نمی‌شود)
✗ فیلترهای Action بعدیِ همان Action اجرا نمی‌شوند
```

 ۲) قطع توسط `TokenAuthorizationFilter` روی Edit: 

```text
→ بدون کوکی معتبر: 401
✗ هیچ Action Filter / Result Filter / رندری اجرا نمی‌شود
```

 ۳) قطع توسط `FeatureDisabledResourceFilter` (isDisabled=true): 

```text
→ Resource Filter با 501 قطع می‌کند
✗ Action Filterها و Action و Result Filterها اجرا نمی‌شوند
✓ پیام after همان Resource Filter اجرا می‌شود (کد بعد از if/else)
```

 ۴) بدون قطع — مسیر عادی Index: 

```text
PersonsListActionFilter.OnActionExecuting      (Order=4)
ResponseHeaderActionFilter ... - before        (Order=1)
  → Action Method
ResponseHeaderActionFilter ... - after
PersonsListActionFilter.OnActionExecuted
PersonsListResultFilter - before
  → رندر View
PersonsListResultFilter - after
```

## 📊 قوانین طلایی Pipeline

1. ترتیب انواع:  Authorization → Resource → Action(before) → Action Method → Action(after) → Result → اجرای نتیجه → Resource(after) 
2. قطع یعنی «ست کردن `context.Result` و ندادن ادامه»؛ هر چه جلوتر قطع کنید، کار کمتری انجام می‌شود و پاسخ سریع‌تر/ارزان‌تر است.
3. قطع در Action Filter به معنی لغو «مرحله نتیجه» نیست؛ فقط رندرِ نتیجه‌ای که شما ست کردید انجام می‌شود.
4. Exception Filter همیشه «نگهبان» است؛ فقط وقتی استثنایی رخ دهد وارد صحنه می‌شود.

## ⚠️ نکات کلیدی و خطاهای رایج

1.  لاگ‌ها را با خودتان چک کنید:  بهترین تمرین، اجرای پروژه و مقایسه ترتیب لاگ‌ها با جدول بالا است؛ این تصویر ذهنی را جایگزین حدس و گمان می‌کند.
2.  Order را فراموش نکنید:  در میان Action Filterها، همان قواعد Order (پروژه‌های ۰۷/۰۸) حاکم است؛ ترتیب لاگ را با Orderها تفسیر کنید.
3.  Result Filter روی مسیر قطع‌شده هم اجرا می‌شود:  اگر فیلتری برای «فقط مسیر موفق» طراحی شده (مثلاً ست کردن کوکی در TokenResultFilter)، مراقب باشید مسیرهای خطا هم آن را اجرا نمی‌کنند اگر قطع در سطح Authorization/Resource رخ داده باشد — به همین دلیل این نقطه‌ها برای منطق امنیتی مهم‌اند.
4.  این پروژه مبنای پروژه‌های بعدی است:  ۱۶ (فیلتری که حتی در مسیر قطع اجرا می‌شود) و ۱۷ (فیلتری که خودش را از اجرا معاف می‌کند) مستقیماً روی همین Pipeline بنا شده‌اند.

## ✅ خلاصه

Short-Circuiting یک «دروازه» در Pipeline است: هر چه جلوتر ببندید، کارهای کمتری انجام می‌شود. این پروژه با ترکیب هر پنج فیلتر، رفتار قطع در هر سطح را به‌صورت عملی نشان داد و یک نقشه ذهنی کامل از ترتیب اجرا ساخت. اگر این نقشه را داشته باشید، طراحی فیلتر در پروژه‌های واقعی ساده می‌شود.
