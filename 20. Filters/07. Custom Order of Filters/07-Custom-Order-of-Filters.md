# پروژه ۰۷ — Custom Order of Filters (ترتیب سفارشی فیلترها)

> منبع: پوشه `20. Filters/07. Custom Order of Filters`
> پیش‌نیاز: پروژه ۰۶ (فیلتر سراسری و سه سطح اعمال)

---

## 🎯 مبحث این پروژه چیست؟

وقتی چند فیلتر از  یک نوع  Action Filter در Pipeline وجود دارند
، ASP.NET Core به‌طور پیش‌فرض بر اساس دامنه (Global → Controller → Action)
و ترتیب تعریف آن‌ها را اجرا می‌کند. این پروژه یاد می‌دهد با پراپرتی  `Order`  در Attribute، این ترتیب را به‌صورت دستی کنترل کنید:

```csharp
[TypeFilter(typeof(ResponseHeaderActionFilter),
    Arguments = new object[] { "K", "V" }, Order = 1)]
```

## 💡 چرا به این مبحث نیاز داریم؟

ترتیب اجرای فیلترها گاهی حیاتی است:
مثلاً فیلتر «اعتبارسنجی ورودی» باید قبل از فیلتر «لاگ» اجرا شود، یا فیلتری که 
Header نهایی را می‌نویسد باید آخرِ کار باشد.
وقتی به سطوح و ترتیب تعریف اعتماد نمی‌کنید، `Order` رفتار را  قطعی و مستند  می‌کند. قاعده طلایی:

>  Order کوچک‌تر → در مرحله Before زودتر، در مرحله After دیرتر. 
> (فیلترها مثل یک نان‌ساندویچ wrap می‌شوند: کوچک‌ترین Order، بیرونی‌ترین لایه است.)

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Controllers/PersonsController.cs`

```csharp
[Route("[controller]")]
[TypeFilter(typeof(ResponseHeaderActionFilter),
    Arguments = new object[] { "My-Key-From-Controller", "My-Value-From-Controller" },
    Order = 2)]                                       // ← سطح Controller، ترتیب 2
public class PersonsController : Controller
{
 [Route("[action]")]
 [Route("/")]
 [TypeFilter(typeof(PersonsListActionFilter))]        // Order پیش‌فرض = 0
 [TypeFilter(typeof(ResponseHeaderActionFilter),
    Arguments = new object[] { "MyKey-FromAction", "MyValue-From-Action" },
    Order = 1)]                                       // ← سطح Action، ترتیب 1
 public async Task<IActionResult> Index(...) { ... }
}
```

و فیلتر Global همچنان در Program.cs ثبت است (Order پیش‌فرض = 0).

## ⚙️ تحلیل ترتیب اجرا

برای Action یعنی `Index`، سه Action Filter داریم:

| فیلتر | سطح | Order | اجرای Before | اجرای After |
|---|---|---|---|---|
| ResponseHeaderActionFilter | Action |  1  | اول | آخر |
| ResponseHeaderActionFilter | Controller |  2  | دوم | دوم |
| ResponseHeaderActionFilter (Global) | Global |  0  | سوم | اول |
| PersonsListActionFilter | Action |  0  (پیش‌فرض) | چهارم (هم‌سطح Global) | — |

خروجی لاگ تقریبی برای `Index`:

```text
ResponseHeaderActionFilter.OnActionExecuting   (Order=1 → قبل از همه)
ResponseHeaderActionFilter.OnActionExecuting   (Order=2)
ResponseHeaderActionFilter.OnActionExecuting   (Global, Order=0 → چون Order=1 و 2 جلویش را گرفتند)
PersonsListActionFilter.OnActionExecuting
  → Action Method
PersonsListActionFilter.OnActionExecuted
ResponseHeaderActionFilter.OnActionExecuted    (Global)
ResponseHeaderActionFilter.OnActionExecuted    (Order=2)
ResponseHeaderActionFilter.OnActionExecuted    (Order=1 → آخرین After)
```

 نکته ظریف:  وقتی فیلتری Order صریح داشته باشد، از فیلترهای بدون Order (پیش‌فرض 0) هم «جلوتر» می‌افتد؛ به همین دلیل فیلترِ Order=1 حتی از Global هم زودتر اجرا می‌شود. (در نسخه‌های جدید ASP.NET Core فیلترهای بدون Order بعد از فیلترهای دارای Order بررسی می‌شوند.)

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `Order` در `[TypeFilter]` | تعیین ترتیب اجرای فیلتر در Pipeline |
| قاعده مرتب‌سازی | اول بر اساس Order صعودی، سپس دامنه (Global→Controller→Action) |
| رفتار Before/After | Before: صعودی؛ After: نزولی (LIFO مثل Stack) |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  Order منفی مجاز است:  می‌توانید `Order = -1` بدهید تا فیلتر حتی قبل از همه Order=0 ها اجرا شود.
2.  Order روی خود Attribute و روی فیلتر سراسری هم کار می‌کند:  در پروژه بعدی (۰۸) Order را داخل خود کلاس فیلتر پیاده می‌کنیم (`IOrderedFilter`).
3.  Order فقط بین فیلترهای «هم‌نوع» مهم است:  ترتیب میان انواع مختلف فیلتر (Authorization → Resource → Action → Result) توسط خود Pipeline تعیین می‌شود و Order نمی‌تواند مثلاً یک Action Filter را قبل از Authorization Filter بگذارد.
4.  شلوغ نکنید:  اگر ترتیب برایتان مهم نیست، Order ننویسید؛ استفاده بی‌دلیل از Order، خواندن Pipeline را سخت می‌کند. آن را فقط برای وابستگی‌های واقعی به‌کار ببرید.

## ✅ خلاصه

پراپرتی `Order` کنترل دستی ترتیب اجرای فیلترهای هم‌نوع است: کوچک‌ترین Order بیرونی‌ترین لایه است — زودترین Before و دیرترین After. این پروژه نشان داد سطوح (Global/Controller/Action) را می‌توان با Order بازآرایی کرد و برای وابستگی‌های منطقی میان فیلترها، ترتیب قطعی ایجاد کرد.
