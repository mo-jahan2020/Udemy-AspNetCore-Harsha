# مقایسه ساختار کدنویسی دو پروژه ASP.NET Core

## پروژه ۱ «Error Handling / UseExceptionHandler» در برابر پروژه ۲ «Clean Architecture / Tests»

>  منبع:  مخزن `mo-jahan2020/Udemy-AspNetCore-Harsha` — هر دو، نسخه‌های تکاملیافتهٔ  یک اپلیکیشن CRUD واحد  (مدیریت Persons و Countries) از یک دورهٔ آموزشی ASP.NET Core هستند. فایل‌های `Program.cs`، Middleware مدیریت خطا، فیلترها و Viewها در هر دو  تقریباً یکسان ‌اند؛ بنابراین تنها تفاوت واقعی،  معماری Solution و نحوهٔ سازماندهی کد  است. همین موضوع آن‌ها را به یک جفت نمونهٔ ایده‌آل برای مقایسهٔ مستقیم دو سبک معماری تبدیل می‌کند.

---

## ۱. نگاه کلی

| مشخصه | پروژه ۱ — Error Handling | پروژه ۲ — Clean Architecture |
|---------|------------------------|------------------------------|
| الگوی معماری |   لایه‌ای  (N-Tier  |Layered) | Clean Architecture |
| نام Solution | `CRUDSolution.sln` | `ContactsManagerSolution.sln` |
| تعداد پروژه‌ها |  ۸ پروژه  (۶ کتابخانه + وب + تست) |  ۶ پروژه  (۳ اصلی + ۳ تست) |
| تیم فریم‌ورک | net10.0 / EF Core 10 / xUnit | net10.0 / EF Core 10 / xUnit |
| اینترفیس سرویس‌ها | ۲ اینترفیس بزرگ (Fat Interface) | ۸ اینترفیس باریک (ISP) |
| محل تست‌ها | ۱ پروژه تست واحد و ترکیبی | ۳ پروژه تست تفکیک‌شده |

هر دو پروژه از نظر فنی هم‌خانواده‌اند:
همان Serilog، همان Rotativa برای PDF، همان فیلترهای Action/Result/Exception و همان Middleware 
خطا. اما پروژهٔ اول کلاس‌ها را «به تفکیک نوعِ لایه» در پروژه‌های جدا پخش کرده، در حالی که پروژهٔ دوم منطق کسب ‌و کار را در یک هستهٔ منسجم به نام
 Core  جمع کرده و جزئیات فنی EF Core ، دیتابیس را به لایهٔ  Infrastructure  رانده است. در ادامه هر کدام را باز می‌کنیم.

---

## ۲. پروژه ۱: معماری لایه‌ای چندپروژه‌ای (N-Tier)

### ۲.۱ ساختار Solution

```
CRUDSolution.sln
├── CRUDExample/            ← اپلیکیشن وب MVC (Controllers, Views, Filters, Middleware)
├── CRUDTests/              ← همهٔ تست‌ها در یک پروژه (Service + Controller + Integration)
├── Entities/               ← کلاس‌های دامنه + ApplicationDbContext + Migrations + پکیج EF Core!
├── RepositoryContracts/    ← اینترفیس‌های ریپازیتوری (ICountriesRepository, IPersonsRepository)
├── Repositories/           ← پیاده‌سازی EF Core ریپازیتوری‌ها
├── ServiceContracts/       ← اینترفیس سرویس‌ها (IPersonsService, ICountriesService) + DTO + Enum
├── Services/               ← پیاده‌سازی سرویس‌ها (PersonsService: ~۳۱۵ خط!)
└── Exceptions/             ← فقط یک کلاس (InvalidPersonIDException)
```

### ۲.۲ جهت وابستگی‌ها (Dependency Graph)

```
CRUDExample (Web) ──► تمام ۶ کتابخانه
Services ──► ServiceContracts + RepositoryContracts + Exceptions
Repositories ──► RepositoryContracts + Entities
ServiceContracts / RepositoryContracts ──► Entities
Entities ──► پکیج Microsoft.EntityFrameworkCore.SqlServer ⚠️
CRUDTests ──► CRUDExample + Services + ServiceContracts + Entities
```

### ۲.۳ ویژگی‌های ساختاری کلیدی

*  هر لایه = یک پروژهٔ جدا.  مرزهای کامپایل‌ زمانی سخت‌گیرانه‌ای وجود دارد؛ مثلاً `Services` اصلاً نمی‌تواند مستقیماً `Repositories` را ببیند و مجبور است از اینترفیس‌های `RepositoryContracts` استفاده کند — همین قید، تزریق وابستگی و Mock کردن را ممکن می‌کند.
*  اینترفیس بزرگ (Fat Interface):  اینترفیس `IPersonsService` یکجا ۸ مسئولیت دارد — افزودن، حذف، ویرایش، جست‌وجو، فیلتر، مرتب‌سازی و حتی تولید خروجی CSV و Excel. کنترلر فقط به دو وابستگی (`IPersonsService` و `ICountriesService`) تزریق می‌کند، اما عملاً به همهٔ آن ۸ قابلیت دسترسی دارد حتی اگر به آن‌ها نیاز نداشته باشد.
*  آلودگی زیرساخت به لایهٔ دامنه:  پروژهٔ `Entities` علاوه بر کلاس‌های POCO، شامل `ApplicationDbContext`، تمام فولدر Migrations و پکیج EF Core است؛ یعنی هستهٔ دامنه از دیتابیس جدا نیست.
*  نشت وب به قراردادها:  پروژهٔ `ServiceContracts` برای دسترسی به `IFormFile` پکیج `Microsoft.AspNetCore.Http` را رفرنس گرفته است؛ یعنی لایهٔ قراردادها به مفاهیم HTTP وابسته شده است.

### ۲.۴ محاسن ✅

1.  سادگی و سرعت یادگیری:  نام‌گذاری پروژه‌ها (Entities، Services، Repositories، ...) دقیقاً با چیزی که در کتاب‌ها گفته می‌شود یکی است و ذهن تازه‌کار خیلی سریع نقشهٔ پروژه را می‌گیرد.
2.  مرزهای فیزیکی بین لایه‌ها:  چون هر لایه یک `.csproj` مستقل است، خطای «استفادهٔ نابجا» (مثلاً صدا زدن Repository از Controller) در زمان کامپایل شناسایی می‌شود.
3.  تست‌پذیری پایه فراهم است:  سرویس‌ها از طریق اینترفیس به ریپازیتوری وصل‌اند و در `PersonsServiceTest` با Moq به‌راحتی Mock شده‌اند؛ تست‌های Integration هم با `WebApplicationFactory` و دیتابیس InMemory کار می‌کنند.
4.  قابلیت استفادهٔ مجدد جزئی:  می‌توان مثلاً فقط `Entities` یا `ServiceContracts` را در پروژهٔ دیگری رفرنس کرد بدون کشیدن کل اپلیکیشن.
5.  سربار DevOps کمتر:  تعداد فایل کمتر، DI ساده‌تر (فقط ۴ ثبت سرویس) و onboarding اعضای جدید سریع‌تر است.

### ۲.۵ معایب ❌

1.  نقض اصل تفکیک اینترفیس (ISP):  هر تغییری در هر قابلیت (حتی فقط خروجی Excel) به اینترفیس `IPersonsService` و همهٔ مصرف‌کنندگانش ضربه می‌زند؛ کلاس `PersonsService` با ~۳۱۵ خط و ۸ متد، هم SRP را خم می‌کند هم تست‌های آن را حجیم.
2.  عدم استقلال از زیرساخت:  چون DbContext و پکیج EF داخل پروژهٔ `Entities` است، لایهٔ دامنه بدون SQL Server قابل تصور نیست و مهاجرت به یک Provider دیگر (مثلاً PostgreSQL) کل پروژه را متأثر می‌کند.
3.  یک پروژهٔ تستِ قاطی:  تست‌های واحد سرویس، تست Controller و تست Integration همه در `CRUDTests` هستند؛ اجرای سریع تست‌های واحد (که نباید به وب‌سرور وابسته باشند) از تست‌های سنگین جدا نیست.
4.  مشکلات مقیاس:  با رشد پروژه، ۸ پروژه به‌سرعت به ۱۲–۱۵ پروژه می‌رسد (Approval، Notification، ...) و مدیریت Solution و رفرنس‌های متقاطع دردسرساز می‌شود.
5.  عدم شفافیت مرز منطقی:  مرز میان «قرارداد سرویس» و «قرارداد ریپازیتوری» فقط اسمی است؛ هیچ مرزی مالکیت‌محور (Ownership) بین Domain و Application وجود ندارد.

---

## ۳. پروژه ۲: معماری Clean Architecture

### ۳.۱ ساختار Solution

```
ContactsManagerSolution.sln
├── ContactsManager.Core/              ← هستهٔ بدون وابستگی
│   ├── Domain/Entities/               ← کلاس‌های دامنه (Person, Country)
│   ├── Domain/RepositoryContracts/    ← اینترفیس‌های ریپازیتوری (مالکیت: Core)
│   ├── ServiceContracts/              ← ۸ اینترفیس باریک سرویس
│   ├── Services/                      ← ۱۰ کلاس سرویس کوچک و متمرکز
│   ├── DTO/ · Exceptions/ · Enums/ · Helpers/
├── ContactsManager.Infrastructure/    ← جزئیات فنی
│   ├── DbContext/ + Migrations/       ← ApplicationDbContext + EF Core
│   └── Repositories/                  ← پیاده‌سازی اینترفیس‌های Core
├── ContactsManager.UI/                ← اپلیکیشن وب MVC (Controllers, Views, Filters)
├── ContactsManager.ServiceTests/      ← تست واحد سرویس‌ها (فقط → Core)
├── ContactsManager.ControllerTests/   ← تست واحد کنترلرها (→ UI)
└── ContactsManager.IntegrationTests/  ← تست انتگرال (→ UI + InMemory)
```

### ۳.۲ قانون وابستگی (Dependency Rule)

```
ContactsManager.UI ──► Core  +  Infrastructure
ContactsManager.Infrastructure ──► Core
ContactsManager.Core ──► هیچ پروژه‌ای ❗ (فقط پکیج‌های NuGet مستقل)
```

قلب این معماری همین است:  Core هیچ رفرنس پروژه‌ای ندارد  و اینترفیس‌های ریپازیتوری داخل خودِ Core تعریف شده‌اند؛
Infrastructure فقط آن‌ها را پیاده می‌کند. یعنی جهت وابستگی به‌جای اینکه به سمت دیتابیس برود،  به سمت داخل (Core) معکوس شده است  —
همان Dependency Inversion واقعی. تست‌های سرویس هم فقط Core را رفرنس می‌گیرند و بدون حتی یک خط EF Core کامپایل و اجرا می‌شوند.

### ۳.۳ ویژگی‌های ساختاری کلیدی

*  تفکیک اینترفیس‌ها (ISP به‌جاما):  به‌جای `IPersonsService`، هشت اینترفیس متمرکز وجود دارد: `IPersonsAdderService` (فقط ۱ متد!)، `IPersonsDeleterService`، `IPersonsUpdaterService`، `IPersonsGetterService`، `IPersonsSorterService`، `ICountriesAdderService`، `ICountriesGetterService` و `ICountriesUploaderService`.
*  چند پیاده‌سازی برای یک قرارداد:  `IPersonsGetterService` سه پیاده‌سازی دارد (`PersonsGetterService`، `PersonsGetterServiceChild` و `PersonsGetterServiceWithFewExcelFields`) و در DI نسخهٔ «WithFewExcelFields» به‌عنوان پیش‌فرض ثبت شده — بدون کوچک‌ترین تغییری در Core. این یعنی جابه‌جایی رفتار در زمان ثبت، نه در زمان کدنویسی.
*  حالت حداقلی از کد (برای درک تفاوت): 

```csharp
// پروژه ۱ — یک اینترفیس بزرگ با ۸ مسئولیت:
public interface IPersonsService {
    Task<PersonResponse> AddPerson(PersonAddRequest? req);
    Task<List<PersonResponse>> GetAllPersons();
    /* ... جست‌وجو، فیلتر، مرتب‌سازی، ویرایش، حذف ... */
    Task<MemoryStream> GetPersonsCSV();
    Task<MemoryStream> GetPersonsExcel();      // ← همه چیز زیر یک سقف
}

// پروژه ۲ — اینترفیس‌های باریک، هر کدام یک دلیل برای تغییر:
public interface IPersonsAdderService {
    Task<PersonResponse> AddPerson(PersonAddRequest? req);   // فقط ۱ متد
}
public interface IPersonsGetterService {
    Task<List<PersonResponse>> GetAllPersons();
    Task<PersonResponse?> GetPersonByPersonID(Guid? personID);
    Task<MemoryStream> GetPersonsCSV();
    Task<MemoryStream> GetPersonsExcel();
}
```

*  ثبت DI مقایسه‌ای: 

```csharp
// پروژه ۱: ۴ ثبت ساده
services.AddScoped<IPersonsService, PersonsService>();
services.AddScoped<ICountriesService, CountriesService>();

// پروژه ۲: ثبت اینترفیس‌های باریک + انتخاب پیاده‌سازی دلخواه
services.AddScoped<IPersonsGetterService, PersonsGetterServiceWithFewExcelFields>();
services.AddScoped<PersonsGetterService, PersonsGetterService>();  // پیاده‌سازی جایگزین
services.AddScoped<IPersonsAdderService, PersonsAdderService>();
services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
```

### ۳.۴ محاسن ✅

1.  استقلال واقعی هسته:  Core بدون EF، بدون دیتابیس و بدون وب کامپایل می‌شود؛ سرویس‌ها می‌توانند روی هر Provider یا حتی بدون دیتابیس (با Mock) تست شوند و منطق کسب‌وکار از جزئیات فنی کاملاً جدا است.
2.  تست‌پذیری عالی و لایه‌بندی‌شده:  پروژهٔ `ServiceTests` فقط Core را رفرنس دارد (تست‌های سریع و ایزوله)، `ControllerTests` با Mock کردن ۶ اینترفیس باریک رفتار کنترلر را تست می‌کند و `IntegrationTests` جریان کامل HTTP را با InMemory DB می‌سنجد — هر نوع تست، ابزار و سربار مخصوص خودش را دارد.
3.  نگهداری و توسعهٔ بلندمدت راحت‌تر:  تغییر دیتابیس فقط در Infrastructure است، تغییر قواعد کسب‌وکار فقط در Core است و تغییر رابط کاربری فقط در UI؛ سه‌گانه‌ای که تعارض پروژه‌های تیمی را به حداقل می‌رساند.
4.  کلاس‌های کوچک و متمرکز:  `PersonsAdderService` حدود ۶۰ خط است در برابر `PersonsService` با ~۳۱۵ خط در پروژهٔ ۱؛ خواندن، بازبینی (Code Review) و Merge کد موازی در تیم بسیار کم‌تنش‌تر است.
5.  انطباق با استاندارد صنعت:  ساختار Core / Infrastructure / UI دقیقاً همان قالبی است که در قالب‌های معروف تیمی (مانند الگوی Jason Taylor برای .NET) استفاده می‌شود؛ انتقال دانش و نیرو بین پروژه‌ها آسان‌تر می‌شود.

### ۳.۵ معایب ❌

1.  منحنی یادگیری بالاتر:  مفاهیمی مثل Dependency Inversion، ISP و سازماندهی پوشه‌های Domain/DTO/Contracts برای تازه‌کار انتزاعی است و تا مدتی «چرا این‌قدر فایل؟» پرسش اصلی خواهد بود.
2.  Boilerplate بیشتر:  ۸ اینترفیس + ۱۰ کلاس سرویس + ۶ وابستگی در سازندهٔ کنترلر، در مقایسه با ۲ سرویس پروژهٔ ۱ حجم تنظیمات و کد تکراری را به‌وضوح بالا می‌برد.
3.  خالص‌بودن نسبی (نه صددرصد):  Core هنوز پکیج‌های Serilog، EPPlus، CsvHelper و Newtonsoft را رفرنس دارد و Entities همچنان با اتریبیوت‌های EF (`[Key]`، `[StringLength]`) تزئین شده‌اند — یک مصالحهٔ عمل‌گرایانه، نه Clean Architecture دقیق آکادمیک.
4.  بوی کد جزئی:  مثلاً در `PersonsAdderService` نوع لاگر به `ILogger<PersonsGetterService>` (کلاس دیگر) تایپ شده که نشانهٔ کپی‌پیست سریع حین refactor است؛ در به‌کارگیری واقعی باید چنین ناهمسانی‌ها را پاکسازی کرد.
5.  حساسیت به ثبت DI:  با زیاد شدن اینترفیس‌ها، فراموشی یک `AddScoped` فقط در زمان اجرا (نه کامپایل) خودش را نشان می‌دهد و دیباگ را کمی دشوارتر می‌کند.

---

## ۴. مقایسه مستقیم، معیار به معیار

| معیار | پروژه ۱ (N-Tier) | پروژه ۲ (Clean Architecture) |
|---|---|---|
| سازماندهی پروژه‌ها | ۸ پروژه، مرزبندی «نوعِ فنی» (Entities/Services/...) | ۳ پروژه، مرزبندی «مسئولیت» (Core/Infra/UI) |
| مالکیت اینترفیس‌های ریپازیتوری | پروژهٔ جداگانه `RepositoryContracts` | داخل خود Core (`Domain/RepositoryContracts`) |
| محل DbContext و Migrations | پروژهٔ `Entities` ⚠️ | پروژهٔ `Infrastructure` ✅ |
| طراحی اینترفیس سرویس | ۲ اینترفیس بزرگ (۸ متد در `IPersonsService`) | ۸ اینترفیس باریک (تک‌مسئولیت) |
| اندازهٔ کلاس سرویس | `PersonsService` ~۳۱۵ خط، ۸ متد | کلاس‌های ~۶۰ خطی تک‌منظوره (۱۰ کلاس) |
| تنوع پیاده‌سازی | هر اینترفیس = فقط ۱ پیاده‌سازی | `IPersonsGetterService` با ۳ پیاده‌سازی قابل تعویض |
| تست‌ها | ۱ پروژهٔ واحد (`CRUDTests`، ~۵۰ تست مخلوط) | ۳ پروژهٔ تفکیک‌شده (Service / Controller / Integration) |
| رفرنس تست سرویس به زیرساخت | دارد (Entities + Services + حتی وب) | ندارد (ServiceTests فقط → Core) |
| وابستگی هستهٔ دامنه | EF Core داخل `Entities` | صفر رفرنس پروژه در Core |
| برنامه‌نویسی خطا و Middleware | `UseExceptionHandler` + Middleware سفارشی | عیناً همان (بدون تغییر) |
| DI (ثبت سرویس‌ها) | ۴ ثبت ساده | ۸+ ثبت اینترفیس باریک |
| مناسب برای | یادگیری، دمو، اپ‌های کوچک | پروژه‌های واقعی، تیمی و بلندمدت |

 نکتهٔ جالب:  تمام زیرساخت‌های جانبی (فیلترها، Middleware خطا، Serilog، Rotativa) در هر دو پروژه یکسان است؛ این نشان می‌دهد مهاجرت از سبک اول به دوم، بیشتر «جابه‌جایی و سازماندهی مجدد» بوده تا بازنویسی منطق.

---

## ۵. جدول امتیازدهی (از ۱۰)

| معیار ارزیابی | پروژه ۱ — N-Tier | پروژه ۲ — Clean Architecture |
|---|:---:|:---:|
| تفکیک مسئولیت‌ها (SRP / ISP) | ۵ |  ۹  |
| تست‌پذیری و ایزوله‌بودن تست‌ها | ۶ |  ۹  |
| استقلال هسته از زیرساخت (EF/DB) | ۴ |  ۹  |
| سادگی شروع و سرعت توسعهٔ اولیه |  ۸  | ۶ |
| خوانایی برای تازه‌کار |  ۸  | ۶ |
| نگهداری و توسعه در بلندمدت | ۶ |  ۹  |
| حجم Boilerplate (کمتر = بهتر) |  ۷  | ۵ |
| انطباق با الگوهای متداول صنعت | ۶ |  ۹  |
|  جمع کل (از ۸۰)  |  ۵۰  |  ۶۲  |

 تفسیر:  پروژهٔ ۲ در همهٔ معیارهای «مهندسی نرم‌افزار بلندمدت» برندهٔ قاطع است، اما پروژهٔ ۱ در معیارهای «هزینهٔ ورود و سادگی» جلوتر است. انتخاب درست بستگی به عمر و مقیاس پروژه دارد، نه به «کدام مدرن‌تر است».

---

## ۶. توصیه‌های عملی: کدام را کجا انتخاب کنیم؟

*  اپ کوچک، دمو، پروژهٔ درسی یا یادگیری مفاهیم پایه  → سبک پروژهٔ ۱ کافی و حتی بهتر است؛ سربار اینترفیس‌های باریک و لایه‌بندی کامل، سرعت شما را کم می‌کند بدون آنکه ارزشی اضافه کند.
*  پروژهٔ تیمی، متوسط تا بزرگ، با عمر بیش از چند ماه  → بدون تردید سبک پروژهٔ ۲؛ هزینهٔ اولیهٔ ساختار، چند برابر در کاهش تعارض‌های Merge، تسهیل Code Review و تست‌های پایدار بازمی‌گردد.
*  در حال یادگیری با این دوره هستید؟  مسیر طبیعی همین است که دوره رفته است: اول N-Tier را کامل بفهمید (سقوط‌پذیری اینترفیس بزرگ را در تست‌ها حس کنید)، بعد ببینید همان کد در Clean Architecture چگونه شکسته و مونتاژ شده است. بهترین تمرین: یکی از فیچرها (مثلاً Upload از Excel) را خودتان از سبک ۱ به سبک ۲ منتقل کنید.
*  می‌خواهید پروژهٔ ۱ را ارتقا دهید؟  سه اقدام پر ارزش: (۱) شکستن `IPersonsService` به اینترفیس‌های باریک، (۲) انتقال `ApplicationDbContext` و Migrations از `Entities` به یک پروژهٔ Infrastructure، (۳) تفکیک `CRUDTests` به سه پروژهٔ تست. همین سه تغییر، ۸۰ درصد فاصلهٔ معماری را پر می‌کند.
*  نکتهٔ تست:  حتی اگر Clean Architecture کامل را نمی‌سازید، تفکیک تست‌های واحد از Integration را همین امروز انجام دهید؛ بیشترین بازگشت سرمایه با کمترین تغییر، همین‌جاست.

---

## ۷. جمع‌بندی

دو پروژه، دو فلسفه‌اند: پروژهٔ ۱ «معماری لایه‌ای کلاسیک» است که یادگیری و شروع را آسان می‌کند اما با بزرگ‌شدن سیستم، اینترفیس‌های بزرگ و آمیختگی دامنه با EF Core به تلهٔ نگهداری تبدیل می‌شود. پروژهٔ ۲ همان منطق را در چارچوب Clean Architecture بازسازی کرده: هسته‌ای مستقل، قراردادهای باریک، زیرساخت جداشده و تست‌های دسته‌بندی‌شده — بهای آن، حجم بیشتر فایل‌ها و انتزاع بیشتر است. برای یادگیری فردی، مسیر ۱ → ۲ دقیقاً همان منحنی رشدی است که یک توسعه‌دهندهٔ حرفه‌ای .NET در صنعت طی می‌کند.

---

## 🇬🇧 English Summary

 What was compared:  Two successive versions of the same CRUD application from an ASP.NET Core course — *21. Error Handling/03. UseExceptionHandler* vs *23. Clean Architecture/04. Tests*.

 Project 1 (Layered / N-Tier):  Eight projects with one class library per technical layer (`Entities`, `RepositoryContracts`, `Repositories`, `ServiceContracts`, `Services`, `Exceptions`) plus the MVC app and a single mixed test project. Business logic flows through repository abstractions, which makes it testable with Moq, and its simplicity is ideal for beginners. Its main weaknesses: a fat `IPersonsService` (8 responsibilities in one interface, ~315-line service class), the EF Core `DbContext` and migrations living inside the `Entities` project (domain coupled to infrastructure), and unit/integration tests mixed together.

 Project 2 (Clean Architecture):  Three projects — `Core` (zero project references; owns entities, repository contracts, 8 narrow service interfaces, 10 focused service classes), `Infrastructure` (EF Core DbContext + repositories implementing Core's interfaces), and `UI` (MVC). Tests are split into `ServiceTests` (references Core only), `ControllerTests`, and `IntegrationTests`. Interface Segregation enables multiple swappable implementations (e.g., `PersonsGetterServiceWithFewExcelFields`). It scores higher on SRP/ISP, testability, infrastructure independence, and long-term maintainability, at the cost of more boilerplate and a steeper learning curve. It is not 100% "pure" — Core still references Serilog/EPPlus/CsvHelper and entities carry EF attributes.

 Verdict:  Clean Architecture version wins 62–50 on the 8-criterion scorecard. Choose the layered style for small apps and learning; choose Clean Architecture for real, team-based, long-lived systems. A pragmatic upgrade path: split the fat interface, move the DbContext to Infrastructure, and separate the test projects.

---
*تحلیل بر اساس اسکن مستقیم سورس‌کد مخزن در تاریخ ۲۰۲۶-۱۰-۲۵ انجام شده است.*

