# پروژه ۰۳ — ViewData in Action Filter  کار با ViewData/ViewBag از داخل فیلتر 

> منبع: پوشه `20. Filters/03. ViewData in Action Filter`
> پیش‌نیاز: پروژه‌های ۰۱ و ۰۲  Action Filter و دسترسی به پارامترها 

---

## 🎯 مبحث این پروژه چیست؟

در این پروژه یاد می‌گیریم که یک Action Filter چگونه می‌تواند به خودِ
Controller  دسترسی پیدا کند و داده‌هایی را برای  View  آماده کند  یعنی مقادیر
`ViewData`/`ViewBag` را از داخل فیلتر مقداردهی کند. همچنین تکنیکی برای  انتقال داده بین دو مرحله
Before و After  یک فیلتر  از طریق `HttpContext.Items`  معرفی می‌شود.

سناریوی پروژه: کدهایی مثل پر کردن `ViewBag.SearchFields` و `ViewBag.CurrentSearchBy` در
Controller بود؛ حالا این مسئولیت به `PersonsListActionFilter` منتقل می‌شود تا
Controller کوچک‌تر و متمرکزتر شود.

## 💡 چرا به این مبحث نیاز داریم؟

وقتی منطق جستجو/مرتب‌سازی به فیلتر منتقل می‌شود، داده‌هایی که
View برای رندر نیاز دارد  مثلاً فهرست فیلدهای قابل جستجو،
مقدار فعلی جستجو و مرتب‌سازی  هم باید در جای درست مقداردهی شوند.
دو مسئله‌ای که این پروژه حل می‌کند:

1.  دسترسی به Controller از فیلتر:  `context.Controller` شیء Controller جاری را می‌دهد؛
  با Cast به نوع مشخص، به `ViewData` و `ViewBag` می‌رسیم.
2.  انتقال اطلاعات از Before به After:  پارامترها فقط در مرحله `OnActionExecuting` در دسترس‌اند؛
  برای اینکه مرحله `OnActionExecuted` هم آن‌ها را داشته باشد،
  آن‌ها را در `HttpContext.Items` ذخیره می‌کنیم  یک Bag کوتاه‌مدت مخصوص همان Request .

## 🔍 کد نمونه از خود پروژه

فایل: `CRUDExample/Filters/ActionFilters/PersonsListActionFilter.cs`

 مرحله قبل — ذخیره پارامترها برای استفاده در مرحله بعد: 

```csharp
public void OnActionExecuting ActionExecutingContext context 
{
 // پارامترهای Action برای استفاده در OnActionExecuted ذخیره می‌شوند
 context.HttpContext.Items["arguments"] = context.ActionArguments;

 // ...  اعتبارسنجی searchBy مثل پروژه قبل 
}
```

 مرحله بعد — دسترسی به Controller و پر کردن ViewData/ViewBag: 

```csharp
public void OnActionExecuted ActionExecutedContext context 
{
 PersonsController personsController =  PersonsController context.Controller;

 IDictionary<string, object?>? parameters =
      IDictionary<string, object?>? context.HttpContext.Items["arguments"];

 if  parameters != null 
 {
  if  parameters.ContainsKey "searchBy"  
   personsController.ViewData["CurrentSearchBy"] = Convert.ToString parameters["searchBy"] ;

  if  parameters.ContainsKey "searchString"  
   personsController.ViewData["CurrentSearchString"] = Convert.ToString parameters["searchString"] ;

  if  parameters.ContainsKey "sortBy"  
   personsController.ViewData["CurrentSortBy"] = Convert.ToString parameters["sortBy"] ;

  if  parameters.ContainsKey "sortOrder"  
   personsController.ViewData["CurrentSortOrder"] = Convert.ToString parameters["sortOrder"] ;
 }

 // پر کردن فهرست فیلدهای جستجو که قبلاً در Controller بود
 personsController.ViewBag.SearchFields = new Dictionary<string, string>  
 {
  { nameof PersonResponse.PersonName , "Person Name" },
  { nameof PersonResponse.Email , "Email" },
  { nameof PersonResponse.DateOfBirth , "Date of Birth" },
  { nameof PersonResponse.Gender , "Gender" },
  { nameof PersonResponse.CountryID , "Country" },
  { nameof PersonResponse.Address , "Address" }
 };
}
```

و در Controller، آن چند خط `ViewBag.SearchFields = ...` و `ViewBag.CurrentSearchBy = ...` حذف شده‌اند؛ فقط منطق سرویس باقی مانده است.

## ⚙️ نحوه کار

```text
Request → OnActionExecuting
             └─ HttpContext.Items["arguments"] = پارامترها
         → Action Method
         → OnActionExecuted
             ├─ context.Controller  →  cast به PersonsController
             ├─ خواندن پارامترها از HttpContext.Items
             └─ ViewData["Current..."] = ... ;  ViewBag.SearchFields = ...
         → View رندر می‌شود و این مقادیر را مصرف می‌کند
```

چرا `HttpContext.Items`؟ چون یک فیلتر، دو متد جدا با دو Context متفاوت دارد  `ActionExecutingContext` و `ActionExecutedContext`  و راه ساده و درست برای گذراندن داده بین آن‌ها، یک دیکشنری هم‌عرض با Request است.

## 📊 جدول APIهای به‌کاررفته

| عضو | نقش |
|---|---|
| `context.Controller` | شیء Controller جاری  نوع `object`؛ باید Cast شود  |
| `Controller.ViewData` | Dictionary `<string, object?>` برای ارسال داده به View |
| `Controller.ViewBag` | Wrapper دینامیک روی ViewData با سینتکس راحت‌تر |
| `HttpContext.Items` | Dictionary کوتاه‌مدت `<object, object?>` مخصوص یک Request؛ پل ارتباطی بین مراحل فیلتر |

## ⚠️ نکات کلیدی و خطاهای رایج

1.  Cast امن‌تر با Pattern Matching:  در این پروژه Cast مستقیم ` PersonsController context.Controller` انجام شده؛ اگر فیلتر را روی Controller دیگری وصل کنید، `InvalidCastException` می‌گیرید. الگوی بهتر: `if  context.Controller is PersonsController p ` — که در پروژه ۱۰ همین کار انجام می‌شود.
2.  ViewData و ViewBag از یک منبع‌اند:  `ViewBag.SearchFields` در واقع همان `ViewData["SearchFields"]` است؛ داخل فیلتر هر کدام را که راحت‌ترید می‌توانید به‌کار ببرید و View هر دو را می‌بیند.
3.  کد View نیازمند هماهنگی است:  View در این پروژه به `ViewData["CurrentSearchBy"]` و غیره ارجاع می‌دهد؛ اگر فیلتر مقدار ندهد  مثلاً پارامتر وجود نداشته باشد ، View باید با مقدار null کنار بیاید — در پروژه ۲۲ راه‌حل «مقدار پیش‌فرض» را می‌بینید.
4.  `HttpContext.Items` فقط برای همین Request است:  بین دو Request منتقل نمی‌شود و جای Session یا Cache نیست؛ عمر آن از شروع تا پایان پردازش همان Request است.

## ✅ خلاصه

فیلترها فقط برای لاگ نیستند؛ می‌توانند قبل از رندر View، داده‌های نمایشی را در `ViewData`/`ViewBag` آماده کنند. کلیدهای این پروژه: دسترسی به Controller از طریق `context.Controller`، و انتقال داده بین مراحل Before/After با `HttpContext.Items`. با این کار مسئولیت «آماده‌سازی داده‌های UI لیست افراد» برای همیشه از Controller جدا و به فیلتر منتقل شد.
