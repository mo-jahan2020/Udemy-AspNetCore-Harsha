# توضیح ساختار پروژه و ارتباط سرویس‌ها و کلاس‌ها

> **مسیر پوشه بررسی‌شده در ریپازیتوری:**
> `14. xUnit / 08. Get Country by Country ID - Implementation`
>
> **منبع:** `https://github.com/mo-jahan2020/Udemy-AspNetCore-Harsha`
>
> **سطح جزئیات:** کد در این پوشه به‌صورت موقت برای توضیح پیاده‌سازی متد `GetCountryByCountryID` در یک معماری لایه‌ای (Layered) ساده با ASP.NET Core و xUnit نوشته شده است.

---

## ۱. نمای کلی Solution

این پوشه حاوی فایل Solution به نام `CRUDSolution.sln` است که شامل **۵ پروژه مجزای .NET 10** می‌باشد. درگاه این فایل، نقشه‌ی پروژه‌ها و وابستگی‌های بین آن‌ها را تعریف می‌کند:

| # | نام پروژه         | نوع پروژه                       | نقش (Responsibility)                                   |
|---|-------------------|----------------------------------|--------------------------------------------------------|
| 1 | `CRUDExample`     | Web (ASP.NET Core MVC – `Microsoft.NET.Sdk.Web`) | میزبان (Host) وب برای اجرای اپلیکیشن                   |
| 2 | `CRUDTests`       | Test Project (xUnit)             | نوشته شدن تست‌های واحد برای سرویس‌ها                   |
| 3 | `Entities`        | Class Library                    | مدل دامنه (Domain Model) و موجودیت‌های پایه            |
| 4 | `ServiceContracts`| Class Library                    | اینترفیس‌های سرویس و DTO‌ها                            |
| 5 | `Services`        | Class Library                    | پیاده‌سازی واقعی سرویس‌ها (Business Logic)             |

### ۱.۱ مشخصات فنی پروژه‌ها

تمام پروژه‌ها از تنظیمات مشابه زیر برخوردارند:

```xml
<TargetFramework>net10.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```

- **Framework:** .NET 10
- **Implicit Usings:** فعال (یعنی using‌های رایج به‌طور خودکار اضافه می‌شوند)
- **Nullable Reference Types:** فعال (مدیریت صریح null‌ها)

پروژه‌ی تست (CRUDTests) به این پکیج‌های NuGet وابسته است:
- `Microsoft.NET.Test.Sdk` نسخه 18.0.1
- `xunit` نسخه 2.9.3
- `xunit.runner.visualstudio` نسخه 3.1.5
- `coverlet.collector` نسخه 6.0.4 (سنجش پوشش کد)

---

## ۲. ساختار درختی پوشه‌ها و فایل‌ها

```text
08. Get Country by Country ID - Implementation/
│
├── CRUDSolution.sln                      # فایل Solution (اشاره به ۵ پروژه)
│
├── Entities/
│   ├── Entities.csproj                   # ارجاع پروژه ندارد (پایه‌ای‌ترین لایه)
│   └── Country.cs                        # مدل دامنه Country
│
├── ServiceContracts/
│   ├── ServiceContracts.csproj           # ارجاع: → Entities
│   ├── ICountriesService.cs              # اینترفیس سرویس Countries
│   └── DTO/
│       ├── CountryAddRequest.cs          # DTO ورودی برای Add
│       └── CountryResponse.cs            # DTO خروجی + متد توسعه (Extension)
│
├── Services/
│   ├── Services.csproj                   # ارجاع: → ServiceContracts (و غیرمستقیم → Entities)
│   └── CountriesService.cs               # پیاده‌سازی ICountriesService
│
├── CRUDExample/
│   ├── CRUDExample.csproj                # وب‌پروژه (SDK = Microsoft.NET.Sdk.Web)
│   ├── Program.cs                        # نقطه‌ی ورود (Startup خلاصه‌شده)
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Properties/
│   │   └── launchSettings.json           # تنظیمات اجرا روی localhost:5298
│   └── wwwroot/
│       └── StyleSheet.css                # فایل استاتیک
│
└── CRUDTests/
    ├── CRUDTests.csproj                  # ارجاع: → Entities, ServiceContracts, Services
    ├── CountriesServiceTest.cs           # کلاس تست‌های CountriesService
    ├── UnitTest1.cs                      # تست نمونه‌ی قدیمی (MyMath)
    └── MyMath.cs                         # کلاس کمکی برای تست ابتدایی
```

---

## ۳. وابستگی‌های بین پروژه‌ها (Project References)

یکی از مهم‌ترین مفاهیم برای فهم ارتباط کلاس‌ها، گراف وابستگی پروژه‌هاست. این وابستگی‌ها به‌طور مستقیم در فایل‌های `.csproj` تعریف شده‌اند و یک زنجیره لایه‌ای تشکیل می‌دهند:

```text
 Entities
   ▲
   │ (ProjectReference)
   ├─── ServiceContracts
   │      ▲
   │      │ (ProjectReference)
   │      ├─── Services
   │      │      ▲
   │      │      │ (ProjectReference)
   │      │      ├─── CRUDTests
   │      │      │      (همچنین به Entities و ServiceContracts هم ارجاع دارد)
   │      │      │
   │      │      └─── CRUDExample (تنها پروژه‌ی وب، فعلاً ارجاعی به Services ندارد)
```

### ۳.۱ جدول وابستگی‌ها

| پروژه             | ارجاع مستقیم به              |
|-------------------|-----------------------------|
| `Entities`         | — (هیچ)                     |
| `ServiceContracts` | `Entities`                  |
| `Services`         | `ServiceContracts`          |
| `CRUDTests`        | `Entities`, `ServiceContracts`, `Services` |
| `CRUDExample`      | — (فعلاً در این قسمت از دوره، رجیستر سرویس انجام نشده است) |

> **نکته‌ی مهم طراحی:** زنجیره‌ی وابستگی به‌صورت لایه‌ای است؛ هر لایه فقط به لایه‌ی پایین‌تر خود ارجاع دارد. این الگو از Circular Reference جلوگیری می‌کند و Maintainability را بالا می‌برد. پروژه‌ی `Services` به‌طور غیرمستقیم از طریق `ServiceContracts` به `Entities` دسترسی دارد.

---

## ۴. شرح تفصیلی هر پروژه و کلاس‌های آن

### ۴.۱ پروژه‌ی `Entities` — مدل دامنه (Domain Layer)

این پروژه پایه‌ای‌ترین لایه است و شامل موجودیت‌های دامنه (Domain Entities) می‌شود. در این پوشه فقط یک کلاس وجود دارد:

#### `Country.cs`

```csharp
namespace Entities
{
  public class Country
  {
    public Guid CountryID { get; set; }
    public string? CountryName { get; set;  }
  }
}
```

**نکات طراحی:**
- `CountryID` از نوع `Guid` است که یک شناسه‌ی یکتای سراسری (Globally Unique Identifier) تولید می‌کند. این انتخاب از `int` مزیت‌هایی دارد: تخصیص غیرمتمرکز، عدم نیاز به هماهنگ‌سازی بین سرورها و مناسب برای محیط‌های توزیع‌شده.
- `CountryName` nullable است (`string?`) تا با قابلیت Nullable Reference Types در C# سازگار باشد و کامپایلر بتواند هشدارهای مربوط به null بودن را بدهد.
- این کلاس Domain Model محسوب می‌شود و نباید به DTO‌ها وابسته باشد.

---

### ۴.۲ پروژه‌ی `ServiceContracts` — قراردادهای سرویس (Contract Layer)

این پروژه دو وظیفه اصلی دارد: تعریف DTO‌ها و تعریف اینترفیس‌های سرویس. این پروژه به `Entities` ارجاع دارد چون DTO‌ها و اینترفیس‌ها با مدل دامنه کار می‌کنند.

#### ۴.۲.۱ `DTO/CountryAddRequest.cs` — DTO ورودی

```csharp
public class CountryAddRequest
{
  public string? CountryName { get; set; }

  public Country ToCountry()
  {
    return new Country() { CountryName = CountryName };
  }
}
```

**نقش:** این DTO برای عملیات «افزودن کشور» به‌کار می‌رود. متد `ToCountry` یک متد تبدیل (Conversion) است که DTO را به مدل دامنه‌ی `Country` تبدیل می‌کند. این الگو جدا نگه داشتن ورودی کاربر از مدل دامنه را تضمین می‌کند (Separation of Concerns).

#### ۴.۲.۲ `DTO/CountryResponse.cs` — DTO خروجی + Extension Method

```csharp
public class CountryResponse
{
  public Guid CountryID { get; set; }
  public string? CountryName { get; set; }

  public override bool Equals(object? obj) { /* مقایسه‌ی فیلدها */ }
  public override int GetHashCode() => base.GetHashCode();

  // ...
}

public static class CountryExtensions
{
  public static CountryResponse ToCountryResponse(this Country country)
  {
    return new CountryResponse() {
      CountryID = country.CountryID,
      CountryName = country.CountryName
    };
  }
}
```

**نکات طراحی:**
- `CountryResponse` DTO خروجی است؛ یعنی چیزی که سرویس به لایه‌های بالاتر (Controller یا Test) برمی‌گرداند.
- متد `Equals` override شده تا بتوان در تست‌ها با `Assert.Equal` دو شیء `CountryResponse` را با هم مقایسه کرد. این نکته در تست `GetCountryByCountryID_ValidCountryID` حیاتی است.
- کلاس استاتیک `CountryExtensions` یک Extension Method به نام `ToCountryResponse` تعریف می‌کند که هر شیء `Country` را به `CountryResponse` تبدیل می‌کند. این یک الگوی زیبا برای نگه‌داشتن منطق تبدیل نزدیک به نوع مقصد است.

#### ۴.۲.۳ `ICountriesService.cs` — اینترفیس سرویس

```csharp
public interface ICountriesService
{
  CountryResponse AddCountry(CountryAddRequest? countryAddRequest);
  List<CountryResponse> GetAllCountries();
  CountryResponse? GetCountryByCountryID(Guid? countryID);
}
```

این اینترفیس قرارداد (Contract) سرویس Countries را تعریف می‌کند و شامل سه متد است:

| متد                       | ورودی                       | خروجی                     | توضیح                            |
|---------------------------|-----------------------------|---------------------------|----------------------------------|
| `AddCountry`              | `CountryAddRequest?`        | `CountryResponse`         | افزودن یک کشور جدید              |
| `GetAllCountries`         | —                           | `List<CountryResponse>`   | برگرداندن همه‌ی کشورها          |
| `GetCountryByCountryID`   | `Guid? countryID`           | `CountryResponse?`        | پیدا کردن کشور با ID مشخص |

> **نکته‌ی کلیدی این قسمت از دوره:** متد `GetCountryByCountryID` تازه به اینترفیس اضافه شده و همین متد در `Services` پیاده‌سازی و در `CRUDTests` تست شده است.

---

### ۴.۳ پروژه‌ی `Services` — پیاده‌سازی سرویس (Implementation Layer)

این پروژه پیاده‌سازی واقعی منطق کسب‌وکار را در خود نگه می‌دارد.

#### `CountriesService.cs`

```csharp
public class CountriesService : ICountriesService
{
  private readonly List<Country> _countries;

  public CountriesService()
  {
    _countries = new List<Country>();
  }

  // متدهای AddCountry, GetAllCountries, GetCountryByCountryID
}
```

**نکات کلیدی پیاده‌سازی:**

1. **ذخیره‌سازی موقت:** داده‌ها در یک لیست in-memory نگه‌داری می‌شوند (`private readonly List<Country> _countries`). این بدان معناست که داده‌ها با هر بار راه‌اندازی مجدد اپلیکیشن از بین می‌روند؛ در قسمت‌های بعدی دوره این پیاده‌سازی با EntityFrameworkCore جایگزین خواهد شد.

2. **پیاده‌سازی `AddCountry`:**
   - سه Validation انجام می‌شود:
     - اگر `countryAddRequest` برابر `null` باشد → `ArgumentNullException`
     - اگر `CountryName` برابر `null` باشد → `ArgumentException`
     - اگر نام کشور تکراری باشد → `ArgumentException`
   - با متد `ToCountry()` تبدیل DTO به مدل دامنه انجام می‌شود
   - با `Guid.NewGuid()` یک شناسه‌ی یکتا تولید می‌شود
   - در نهایت با متد `ToCountryResponse()` نتیجه به DTO خروجی تبدیل می‌شود

3. **پیاده‌سازی `GetAllCountries`:**
   - همه‌ی موجودیت‌های `Country` را به `CountryResponse` تبدیل کرده و برمی‌گرداند
   - با LINQ و `Select` پیاده‌سازی شده است

4. **پیاده‌سازی `GetCountryByCountryID` (محور این پوشه):**
   ```csharp
   public CountryResponse? GetCountryByCountryID(Guid? countryID)
   {
     if (countryID == null)
       return null;

     Country? country_response_from_list =
       _countries.FirstOrDefault(temp => temp.CountryID == countryID);

     if (country_response_from_list == null)
       return null;

     return country_response_from_list.ToCountryResponse();
   }
   ```
   - اگر `countryID` برابر `null` باشد، متد `null` برمی‌گرداند
   - با LINQ و `FirstOrDefault` جستجو انجام می‌شود
   - اگر هیچ موردی یافت نشد، `null` برگردانده می‌شود
   - در غیر این صورت، موجودیت پیدا‌شده به DTO تبدیل و برگردانده می‌شود

---

### ۴.۴ پروژه‌ی `CRUDExample` — وب اپلیکیشن (Host / Web Layer)

این پروژه میزبان (Host) اجرایی است که اپلیکیشن را بالا می‌آورد.

#### `Program.cs` (Minimal Hosting Model)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.MapControllers();

app.Run();
```

**نکات:**
- با الگوی Minimal Hosting Model در .NET نوشته شده (بدون `Startup.cs` جداگانه)
- سرویس‌های MVC با `AddControllersWithViews` رجیستر شده‌اند
- در محیط Development، صفحه‌ی خطای توسعه‌دهنده فعال می‌شود
- فایل‌های استاتیک (مثل `wwwroot/StyleSheet.css`) سرو می‌شوند
- **نکته‌ی مهم:** در این مرحله از دوره، هنوز `ICountriesService` و `CountriesService` در DI Container رجیستر نشده‌اند؛ این کار در قسمت بعدی انجام خواهد شد. به همین دلیل فعلاً در `Program.cs` ارجاعی به پروژه‌ی `Services` وجود ندارد.

#### سایر فایل‌ها:
- `appsettings.json` و `appsettings.Development.json`: تنظیمات Logging
- `Properties/launchSettings.json`: اپلیکیشن روی `http://localhost:5298` اجرا می‌شود
- `wwwroot/StyleSheet.css`: فایل CSS استاتیک

---

### ۴.۵ پروژه‌ی `CRUDTests` — تست‌های واحد (Test Layer)

این پروژه شامل تمام تست‌های واحد است که رفتار سرویس‌ها را می‌سنجد.

#### `CountriesServiceTest.cs` (محور اصلی تست‌ها)

این کلاس تست، رفتار سرویس `CountriesService` را می‌سنجد. ساختار کلی آن به این شکل است:

```csharp
public class CountriesServiceTest
{
  private readonly ICountriesService _countriesService;

  public CountriesServiceTest()
  {
    _countriesService = new CountriesService();
  }

  // تست‌های زیر...
}
```

> **نکته‌ی معماری:** تست‌ها به‌جای استفاده از `CountriesService` مستقیماً، با متغیر از نوع اینترفیس `ICountriesService` کار می‌کنند. این کار از اصل Polymorphism استفاده می‌برد و در آینده امکان Mock کردن را فراهم می‌کند (در قسمت 18 این دوره با کتابخانه‌ی Moq آشنا می‌شوید).

هر تست از الگوی **AAA** پیروی می‌کند:

| مرحله   | معنی            | نقش                                                  |
|---------|-----------------|------------------------------------------------------|
| Arrange | آماده‌سازی      | آماده‌سازی داده‌های ورودی و شرایط تست               |
| Act     | اجرا           | فراخوانی متدی که در حال تست است                       |
| Assert  | اعتبارسنجی     | بررسی اینکه نتیجه‌ی واقعی با انتظار هم‌خوانی دارد     |

#### تست‌های موجود در `CountriesServiceTest`:

این کلاس به سه `#region` تقسیم شده:

**الف) `AddCountry` (۴ تست):**

1. `AddCountry_NullCountry` — اگر ورودی `null` باشد، باید `ArgumentNullException` پرتاب شود
2. `AddCountry_CountryNameIsNull` — اگر `CountryName` برابر `null` باشد، باید `ArgumentException` پرتاب شود
3. `AddCountry_DuplicateCountryName` — اگر نام تکراری باشد، باید `ArgumentException` پرتاب شود
4. `AddCountry_ProperCountryDetails` — در حالت درست، کشور باید با `CountryID` غیرخالی به لیست اضافه شود

**ب) `GetAllCountries` (۲ تست):**

1. `GetAllCountries_EmptyList` — به‌طور پیش‌فرض لیست باید خالی باشد
2. `GetAllCountries_AddFewCountries` — وقتی چند کشور اضافه می‌شود، باید در لیست بازگشتی موجود باشند

**ج) `GetCountryByCountryID` (۲ تست) — محور این پوشه:**

```csharp
[Fact]
public void GetCountryByCountryID_NullCountryID()
{
  Guid? countrID = null;
  CountryResponse? country_response_from_get_method =
    _countriesService.GetCountryByCountryID(countrID);
  Assert.Null(country_response_from_get_method);
}

[Fact]
public void GetCountryByCountryID_ValidCountryID()
{
  CountryAddRequest? country_add_request =
    new CountryAddRequest() { CountryName = "China" };
  CountryResponse country_response_from_add =
    _countriesService.AddCountry(country_add_request);

  CountryResponse? country_response_from_get =
    _countriesService.GetCountryByCountryID(country_response_from_add.CountryID);

  Assert.Equal(country_response_from_add, country_response_from_get);
}
```

**تحلیل تست‌های `GetCountryByCountryID`:**

1. **تست NullCountryID:** ورودی `null` می‌دهد و انتظار خروجی `null` دارد. این تست رفتار کد را وقتی `countryID == null` است می‌سنجد. دلیل استفاده از `Assert.Null` این است که متد بلافاصله `null` برمی‌گرداند و نباید Exception پرتاب کند.

2. **تست ValidCountryID:**
   - **Arrange:** ابتدا یک کشور با نام "China" اضافه می‌کند تا یک `CountryResponse` معتبر داشته باشد
   - **Act:** سپس با `CountryID` همان کشور، متد `GetCountryByCountryID` را فراخوانی می‌کند
   - **Assert:** نتیجه‌ی `Get` را با نتیجه‌ی `Add` مقایسه می‌کند. این کار به‌خاطر override کردن متد `Equals` در `CountryResponse` ممکن می‌شود. اگر `Equals` override نشده بود، مقایسه‌ی رفرنسی انجام می‌شد و تست شکست می‌خورد.

#### سایر فایل‌های تست:

- **`UnitTest1.cs` و `MyMath.cs`:** این دو فایل یک تست نمونه‌ی ابتدایی هستند که در ابتدای دوره نوشته شده‌اند تا مفهوم xUnit را ساده توضیح دهند. کلاس `MyMath` یک متد `Add` ساده دارد که دو عدد را جمع می‌کند. این فایل‌ها ارتباط مستقیمی با منطق کشور ندارند.

---

## ۵. نمودار ارتباط سرویس‌ها و کلاس‌ها

در ادامه، ارتباط کامل کلاس‌ها و سرویس‌ها را در قالب یک نمودار می‌بینید:

```text
┌──────────────────────────────────────────────────────────────────┐
│                         CRUDTests (Tests)                        │
│                                                                  │
│   CountriesServiceTest                                           │
│   - depends on → ICountriesService (Contract)                    │
│   - instantiation → new CountriesService() (Implementation)      │
│   - uses DTO: CountryAddRequest, CountryResponse                │
│                                                                  │
│   UnitTest1 → MyMath (تست نمونه‌ی ابتدایی)                       │
└──────────────────────────────────────────────────────────────────┘
                                  │
                                  │ uses (polymorphism)
                                  ▼
┌──────────────────────────────────────────────────────────────────┐
│                   ServiceContracts (Contracts)                   │
│                                                                  │
│   ┌─────────────────────┐       ┌──────────────────────────┐     │
│   │ ICountriesService   │       │   DTO Folder             │     │
│   │ - AddCountry()      │       │  ├── CountryAddRequest   │     │
│   │ - GetAllCountries() │       │  │   └── ToCountry()     │     │
│   │ - GetCountryBy      │       │  └── CountryResponse     │     │
│   │   CountryID()       │       │      ├── Equals()        │     │
│   └─────────┬───────────┘       │      └── GetHashCode()  │     │
│             │ implements         └──────────────────────────┘     │
│             │                            ▲                        │
│             │                            │ extends                │
│             │                            │                        │
│             │                ┌───────────┴───────────┐           │
│             │                │ CountryExtensions       │           │
│             │                │ - ToCountryResponse()  │           │
│             │                └─────────────────────────┘           │
└─────────────┼────────────────────────────────────────┘             │
              │                                                     │
              │ implements                                          │
              ▼                                                     │
┌──────────────────────────────────────────────────────────────────┐
│                      Services (Implementation)                   │
│                                                                  │
│   CountriesService : ICountriesService                           │
│   ├── private field: List<Country> _countries (in-memory)        │
│   ├── constructor: new List<Country>()                           │
│   ├── AddCountry()      → validation → ToCountry() → add → response│
│   ├── GetAllCountries() → Select(ToCountryResponse)              │
│   └── GetCountryByCountryID() → FirstOrDefault → ToCountryResponse│
│                                                                  │
│   Dependencies:                                                  │
│   ├── Entities.Country                                           │
│   ├── ServiceContracts.DTO.*                                     │
│   └── ServiceContracts.ICountriesService                          │
└──────────────────────────────────────────────────────────────────┘
                                  │
                                  │ uses
                                  ▼
┌──────────────────────────────────────────────────────────────────┐
│                          Entities (Domain)                       │
│                                                                  │
│   Country                                                        │
│   ├── Guid  CountryID                                            │
│   └── string? CountryName                                        │
└──────────────────────────────────────────────────────────────────┘
                                  ▲
                                  │ hosts (but not yet wired)
                                  │
┌──────────────────────────────────────────────────────────────────┐
│                         CRUDExample (Web Host)                   │
│                                                                  │
│   Program.cs (Minimal Hosting)                                   │
│   ├── AddControllersWithViews()  (MVC registered)                │
│   ├── UseStaticFiles()                                          │
│   ├── MapControllers()                                           │
│   └── (Registration of ICountriesService → CountriesService      │
│        will be added in subsequent sections of the course)       │
└──────────────────────────────────────────────────────────────────┘
```

---

## ۶. جریان اجرای متد `GetCountryByCountryID`

برای درک بهتر ارتباط کلاس‌ها، جریان اجرای یک فراخوانی کامل به این متد را مرور می‌کنیم:

```text
1. فراخواننده (مثلاً Test یا Controller)
   │
   │  GetCountryByCountryID(guid)
   ▼
2. ICountriesService.GetCountryByCountryID(countryID)   [Contract]
   │  (polymorphic dispatch)
   ▼
3. CountriesService.GetCountryByCountryID(countryID)    [Implementation]
   │
   ├── 3.1. بررسی null بودن countryID
   │        └── اگر null → return null (با نوع CountryResponse?)
   │
   ├── 3.2. LINQ: _countries.FirstOrDefault(c => c.CountryID == countryID)
   │        └── در لیست in-memory جستجو می‌کند
   │        └── روی Entity از نوع Country کار می‌کند
   │
   ├── 3.3. اگر چیزی پیدا نشد → return null
   │
   └── 3.4. country_response_from_list.ToCountryResponse()
            └── CountryExtensions.ToCountryResponse(this Country)
            └── تبدیل Entity به DTO
            ▼
4. CountryResponse? به فراخواننده برگردانده می‌شود
   │
   │  (در تست: Assert.Equal با override شده‌ی Equals مقایسه می‌کند)
   ▼
5. تست با Assert.Equal یا Assert.Null نتیجه را می‌سنجد
```

---

## ۷. مفاهیم و الگوهای طراحی به‌کار رفته

این پروژه، نمونه‌ی کلاسیک و تمیزی از چند الگوی طراحی و معماری است:

### ۷.۱ معماری لایه‌ای (Layered Architecture)

پروژه به چهار لایه‌ی مجزا تقسیم شده:

1. **لایه‌ی دامنه (Domain Layer):** `Entities`
2. **لایه‌ی قراردادها (Contract Layer):** `ServiceContracts`
3. **لایه‌ی پیاده‌سازی (Implementation Layer):** `Services`
4. **لایه‌ی میزبان (Host Layer):** `CRUDExample`
5. **لایه‌ی تست (Test Layer):** `CRUDTests`

مزایای این الگو:
- جداسازی واضح مسئولیت‌ها (Separation of Concerns)
- قابلیت تست‌پذیری بالا (می‌توان پیاده‌سازی سرویس را با Mock جایگزین کرد)
- قابل تعویض بودن پیاده‌سازی (مثلاً جایگزینی ذخیره‌سازی in-memory با EF Core)
- جلوگیری از Circular Dependency

### ۷.۲ الگوی DTO (Data Transfer Object)

پروژه به‌جای ارسال مستقیم مدل دامنه به بیرون، از DTO‌ها استفاده می‌کند:

- **`CountryAddRequest`** برای ورودی
- **`CountryResponse`** برای خروجی

مزایا:
- پنهان کردن جزئیات داخلی مدل دامنه (Encapsulation)
- امکان تغییر مدل دامنه بدون شکستن API
- جداسازی اعتبارسنجی ورودی از منطق دامنه
- کنترل دقیق روی فیلدهای قابل ارسال

### ۷.۳ الگوی Interface-Based Design (Program to Interface)

سرویس‌ها با اینترفیس (`ICountriesService`) تعریف می‌شوند و پیاده‌سازی (`CountriesService`) جداگانه. تست‌ها هم به اینترفیس ارجاع می‌دهند نه پیاده‌سازی مستقیم. این کار:
- امکان تزریق وابستگی (Dependency Injection) را فراهم می‌کند
- جایگزینی پیاده‌سازی با Mock (در آینده با Moq) را ممکن می‌سازد
- قرارداد روشنی بین لایه‌ها ایجاد می‌کند

### ۷.۴ Extension Methods

استفاده از `CountryExtensions.ToCountryResponse` برای تبدیل، یک الگوی تمیز برای:
- نگه‌داشتن منطق تبدیل نزدیک به نوع مقصد
- کدخوانایی بالاتر (مثل `country.ToCountryResponse()`)
- قابلیت استفاده‌ی مجدد در سراسر کد

### ۷.۵ الگوی AAA در تست‌ها

تمام تست‌های xUnit در این پروژه از الگوی Arrange-Act-Assert پیروی می‌کنند که کد تست را خوانا و قابل نگه‌داری می‌کند.

### ۷.۶ In-Memory Storage (موقت)

پیاده‌سازی فعلی از `List<Country>` به‌عنوان ذخیره‌ساز استفاده می‌کند. این انتخاب برای اهداف آموزشی است و در قسمت‌های بعدی دوره با EntityFrameworkCore جایگزین خواهد شد.

---

## ۸. نکات قابل توجه در این مرحله از دوره

1. **هنوز DI فعال نیست:** در `Program.cs` پروژه‌ی `CRUDExample`، `ICountriesService` رجیستر نشده است. این کار در قسمت بعدی انجام می‌شود.

2. **تست‌ها به‌جای DI، مستقیماً new می‌کنند:** در سازنده‌ی `CountriesServiceTest`، `new CountriesService()` فراخوانی می‌شود. این الگو وقتی منطق ساده است و وابستگی خارجی ندارد، قابل قبول است. در آینده با Moq این الگو تغییر می‌کند.

3. **تنها این پوشه روی `GetCountryByCountryID` تمرکز دارد:** متد `GetCountryByCountryID` تازه به `ICountriesService` اضافه شده و در `CountriesService` پیاده‌سازی و در `CountriesServiceTest` تست شده است. سایر متدها از پوشه‌های قبلی این دوره به ارث رسیده‌اند.

4. **هدف override شدن `Equals` در `CountryResponse`:** این کار فقط برای امکان مقایسه در `Assert.Equal` انجام شده است. بدون این override، مقایسه‌ی رفرنسی انجام می‌شد و تست شکست می‌خورد.

5. **`GetHashCode` به‌سادگی override شده:** این متد به `base.GetHashCode()` ارجاع می‌دهد که در عمل یک انتخاب نه‌چندان اصولی است (وقتی `Equals` override می‌شود، `GetHashCode` هم باید منطق متناظر داشته باشد). این نکته در پروژه‌های واقعی باید اصلاح شود.

6. **بهینه‌سازی احتمالی:** در `AddCountry`، چک تکراری بودن نام کشور با `Where(...).Count() > 0` نوشته شده. استفاده از `Any(...)` کارآمدتر است چون به‌محض پیدا شدن اولین مورد متوقف می‌شود.

---

## ۹. جمع‌بندی

این پوشه، یک قدم دیگر در مسیر تکاملی ساخت یک اپلیکیشن CRUD با ASP.NET Core است. در این مرحله:

- **ساختار Solution** شامل ۵ پروژه‌ی مجزا با وابستگی‌های لایه‌ای است
- **معماری** از نوع Layered با جداسازی مدل دامنه، قرارداد، پیاده‌سازی و میزبان است
- **محور این قسمت** پیاده‌سازی و تست متد `GetCountryByCountryID` است که به جستجوی یک کشور با شناسه‌ی یکتای Guid می‌پردازد
- **الگوهای طراحی** شامل DTO Pattern, Interface-Based Design, Extension Methods و AAA Test Pattern می‌باشد
- **ذخیره‌سازی** فعلاً in-memory است و در آینده با EF Core جایگزین می‌شود
- **تست‌ها** با xUnit و با الگوی AAA نوشته شده‌اند و رفتار سرویس را در حالت‌های مختلف (null, valid, invalid) می‌سنجند

این ساختار، یک نقطه‌ی شروع تمیز برای توسعه‌ی واقعی است که در قسمت‌های بعدی دوره به تدریج با EF Core, Moq, Repository Pattern, Clean Architecture و JWT کامل می‌شود.
