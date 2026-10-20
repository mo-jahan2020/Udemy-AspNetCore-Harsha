# توضیح جامع ساختار پروژه و ارتباط سرویس‌ها و کلاس‌ها

> **منبع:** `https://github.com/mo-jahan2020/Udemy-AspNetCore-Harsha`
>
> **پوشه‌های بررسی‌شده در این سند:**
> 1. `14. xUnit / 08. Get Country by Country ID - Implementation` (Country)
> 2. `14. xUnit / 26. Delete Person - Implementation` (Country + Person)
>
> **هدف:** تجمیع تمام توضیحات مربوط به دو موجودیت **Person** و **Country** در یک سند واحد
>
> **سطح جزئیات:** کد در این پوشه‌ها به‌صورت لایه‌ای (Layered) با ASP.NET Core و xUnit نوشته شده و فرآیند تکاملی یک اپلیکیشن CRUD را از حالت ساده (فقط Country با GetCountryByCountryID) تا حالت کامل (Country + Person با همه‌ی عملیات CRUD شامل DeletePerson) نشان می‌دهد.

---

## فهرست مطالب

1. [مقدمه و نمای کلی دو پوشه](#۱-مقدمه-و-نمای-کلی-دو-پوشه)
2. [ساختار Solution و پروژه‌ها](#۲-ساختار-solution-و-پروژه‌ها)
3. [ساختار درختی فایل‌ها](#۳-ساختار-درختی-فایل‌ها)
4. [وابستگی‌های بین پروژه‌ها](#۴-وابستگی‌های-بین-پروژه‌ها)
5. [لایه‌ی Entities — مدل دامنه](#۵-لایه‌ی-entities--مدل-دامنه)
6. [لایه‌ی ServiceContracts — قراردادها، DTO‌ها و Enum‌ها](#۶-لایه‌ی-servicecontracts--قراردادها-dtoها-و-enumها)
7. [لایه‌ی Services — پیاده‌سازی سرویس‌ها](#۷-لایه‌ی-services--پیاده‌سازی-سرویس‌ها)
8. [لایه‌ی CRUDExample — وب اپلیکیشن](#۸-لایه‌ی-crudexample--وب-اپلیکیشن)
9. [لایه‌ی CRUDTests — تست‌های واحد](#۹-لایه‌ی-crudtests--تستهای-واحد)
10. [نمودار ارتباط کامل کلاس‌ها و سرویس‌ها](#۱۰-نمودار-ارتباط-کامل-کلاسها-و-سرویسها)
11. [جریان اجرای متدهای محوری](#۱۱-جریان-اجرای-متدهای-محوری)
12. [مفاهیم و الگوهای طراحی به‌کار رفته](#۱۲-مفاهیم-و-الگوهای-طراحی-بهکار-رفته)
13. [مقایسه‌ی پوشه‌ی ۰۸ با پوشه‌ی ۲۶](#۱۳-مقایسهی-پوشهی-۰۸-با-پوشهی-۲۶)
14. [نکات قابل توجه و فرصت‌های بهبود](#۱۴-نکات-قابل-توجه-و-فرصتهای-بهبود)
15. [جمع‌بندی نهایی](#۱۵-جمعبندی-نهایی)

---

## ۱. مقدمه و نمای کلی دو پوشه

این سند، تجمیعی از توضیحات کامل دو پوشه از دوره‌ی Udemy-AspNetCore-Harsha است که فرآیند یادگیری و تکامل یک اپلیکیشن CRUD را در ASP.NET Core نشان می‌دهد. **پوشه‌ی اول** (`08. Get Country by Country ID - Implementation`) یک نقطه‌ی شروع ساده است که تمرکز آن بر پیاده‌سازی و تست متد `GetCountryByCountryID` در سرویس Countries است. این پوشه فقط یک موجودیت (`Country`) و یک سرویس را معرفی می‌کند و مناسب یادگیری مفاهیم پایه‌ی معماری لایه‌ای، DTO Pattern و تست‌نویسی با xUnit است.

**پوشه‌ی دوم** (`26. Delete Person - Implementation`) نقطه‌ی پایان مسیر آموزشی CRUD است. در این پوشه، علاوه بر موجودیت `Country`، موجودیت `Person` نیز اضافه شده و تمام عملیات CRUD (Add, GetAll, GetById, Filter, Sort, Update, Delete) برای آن پیاده‌سازی و تست شده است. این پوشه شامل ۲ موجودیت، ۵ DTO، ۲ enum، ۲ سرویس، ۱ helper و ۲ کلاس تست است و نزدیک به یک اپلیکیشن واقعی است.

در این سند، تمام جزئیات هر دو پوشه در کنار هم قرار گرفته‌اند تا یک تصویر کامل از معماری پروژه، ارتباط کلاس‌ها و الگوهای طراحی به دست آید. ساختار سند به‌گونه‌ای طراحی شده که خواننده بتواند هم نسخه‌ی ساده‌ی Country-only و هم نسخه‌ی کامل Person+Country را در کنار هم ببیند و فرآیند تکامل پروژه را درک کند.

---

## ۲. ساختار Solution و پروژه‌ها

هر دو پوشه حاوی فایل Solution به نام `CRUDSolution.sln` است که شامل **۵ پروژه‌ی مجزای .NET 10** می‌باشد. ساختار Solution در هر دو پوشه یکسان است، اما محتوای پروژه‌ها به‌طور قابل‌توجهی متفاوت است.

| # | نام پروژه         | نوع پروژه                       | نقش (Responsibility)                                   |
|---|-------------------|----------------------------------|--------------------------------------------------------|
| 1 | `CRUDExample`     | Web (ASP.NET Core MVC – `Microsoft.NET.Sdk.Web`) | میزبان (Host) وب برای اجرای اپلیکیشن                   |
| 2 | `CRUDTests`       | Test Project (xUnit)             | نوشته شدن تست‌های واحد برای سرویس‌ها                   |
| 3 | `Entities`        | Class Library                    | مدل دامنه (Domain Model) و موجودیت‌های پایه            |
| 4 | `ServiceContracts`| Class Library                    | اینترفیس‌های سرویس، DTO‌ها و Enum‌ها                  |
| 5 | `Services`        | Class Library                    | پیاده‌سازی واقعی سرویس‌ها (Business Logic)             |

### ۲.۱ مشخصات فنی پروژه‌ها

تمام پروژه‌ها از تنظیمات مشابه زیر برخوردارند که نشان‌دهنده‌ی استفاده از جدیدترین ویژگی‌های C# و .NET است:

```xml
<TargetFramework>net10.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```

- **Framework:** .NET 10 — جدیدترین نسخه‌ی پایدار در زمان نگارش این دوره
- **Implicit Usings:** فعال — یعنی using‌های رایج به‌طور خودکار اضافه می‌شوند و کد تمیزتر می‌شود
- **Nullable Reference Types:** فعال — مدیریت صریح null‌ها و هشدارهای کامپایلر

پروژه‌ی تست (`CRUDTests`) در هر دو پوشه به این پکیج‌های NuGet وابسته است:
- `Microsoft.NET.Test.Sdk` نسخه 18.0.1 — SDK اجرای تست
- `xunit` نسخه 2.9.3 — فریمورک تست
- `xunit.runner.visualstudio` نسخه 3.1.5 — runner برای Visual Studio
- `coverlet.collector` نسخه 6.0.4 — سنجش پوشش کد (code coverage)

این انتخاب پکیج‌ها استاندارد و به‌روز است و نشان می‌دهد پروژه از بهترین ابزارهای تست‌نویسی در اکوسیستم .NET استفاده می‌کند.

---

## ۳. ساختار درختی فایل‌ها

### ۳.۲ پوشه‌ی ۲۶ (نسخه‌ی کامل — Country + Person)

این پوشه نسخه‌ی کامل‌تر و نزدیک به اپلیکیشن واقعی است. تمام کلاس‌های موجودیت `Person`، DTO‌های Person، Enum‌ها، ValidationHelper و PersonsService اضافه شده‌اند:

```text
26. Delete Person - Implementation/
│
├── CRUDSolution.sln                              # فایل Solution (اشاره به ۵ پروژه)
│
├── Entities/
│   ├── Entities.csproj                           # ارجاع پروژه ندارد
│   ├── Country.cs                                # مدل دامنه Country
│   └── Person.cs                                 # مدل دامنه Person (جدید)
│
├── ServiceContracts/
│   ├── ServiceContracts.csproj                   # ارجاع: → Entities
│   ├── ICountriesService.cs                      # اینترفیس سرویس Countries
│   ├── IPersonsService.cs                         # اینترفیس سرویس Persons (جدید)
│   ├── DTO/
│   │   ├── CountryAddRequest.cs                  # DTO ورودی برای Add Country
│   │   ├── CountryResponse.cs                    # DTO خروجی Country + Extension
│   │   ├── PersonAddRequest.cs                   # DTO ورودی برای Add Person (جدید)
│   │   ├── PersonUpdateRequest.cs                # DTO ورودی برای Update Person (جدید)
│   │   └── PersonResponse.cs                     # DTO خروجی Person + Extension (جدید)
│   └── Enums/
│       ├── GenderOptions.cs                      # Male, Female, Other (جدید)
│       └── SortOrderOptions.cs                   # ASC, DESC (جدید)
│
├── Services/
│   ├── Services.csproj                           # ارجاع: → ServiceContracts
│   ├── CountriesService.cs                       # پیاده‌سازی ICountriesService
│   ├── PersonsService.cs                         # پیاده‌سازی IPersonsService (جدید)
│   └── Helpers/
│       └── ValidationHelper.cs                   # کمکی اعتبارسنجی مدل (جدید)
│
├── CRUDExample/
│   ├── CRUDExample.csproj                        # وب‌پروژه
│   ├── Program.cs                                # نقطه‌ی ورود
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Properties/launchSettings.json
│   └── wwwroot/StyleSheet.css
│
└── CRUDTests/
    ├── CRUDTests.csproj                          # ارجاع: → Entities, ServiceContracts, Services
    ├── CountriesServiceTest.cs                   # تست‌های CountriesService
    ├── PersonsServiceTest.cs                     # تست‌های PersonsService (جدید)
    ├── UnitTest1.cs                              # تست نمونه‌ی قدیمی
    └── MyMath.cs                                 # کلاس کمکی
```

### ۳.۳ فهرست کلاس‌ها به تفکیک پروژه

جدول زیر تمام کلاس‌ها و فایل‌های موجود در هر دو پوشه را نشان می‌دهد و علامت `(۲۶)` نشان می‌دهد که آن کلاس فقط در پوشه‌ی ۲۶ وجود دارد:

| پروژه             | کلاس‌ها و فایل‌ها                                                                              |
|-------------------|-------------------------------------------------------------------------------------------------|
| `Entities`         | `Country`, `Person (۲۶)`                                                                       |
| `ServiceContracts` | `ICountriesService`, `IPersonsService (۲۶)`, `CountryAddRequest`, `CountryResponse`, `PersonAddRequest (۲۶)`, `PersonUpdateRequest (۲۶)`, `PersonResponse (۲۶)`, `GenderOptions (۲۶)`, `SortOrderOptions (۲۶)`, `CountryExtensions`, `PersonExtensions (۲۶)` |
| `Services`         | `CountriesService`, `PersonsService (۲۶)`, `ValidationHelper (۲۶)`                              |
| `CRUDExample`      | `Program` (Top-level statements)                                                               |
| `CRUDTests`        | `CountriesServiceTest`, `PersonsServiceTest (۲۶)`, `UnitTest1`, `MyMath`                       |

---

## ۴. وابستگی‌های بین پروژه‌ها

گراف وابستگی پروژه‌ها در هر دو پوشه یکسان است و الگوی لایه‌ای معروفی دارد. هر لایه فقط به لایه‌ی پایین‌تر خود ارجاع دارد. این الگو از Circular Reference جلوگیری می‌کند و قابلیت نگه‌داری (Maintainability) را بالا می‌برد.

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
   │      │      └─── CRUDExample (تنها پروژه‌ی وب، فعلاً در ۲ پوشه DI برای سرویس‌ها فعال نیست)
```

### ۴.۱ جدول وابستگی‌ها

| پروژه             | ارجاع مستقیم به                              |
|-------------------|-----------------------------------------------|
| `Entities`         | — (هیچ)                                       |
| `ServiceContracts` | `Entities`                                    |
| `Services`         | `ServiceContracts` (و غیرمستقیم `Entities`)  |
| `CRUDTests`        | `Entities`, `ServiceContracts`, `Services`    |
| `CRUDExample`      | — (فعلاً در هر دو پوشه، DI برای سرویس‌ها فعال نیست) |

> **نکته‌ی مهم طراحی:** پروژه‌ی `Services` به‌طور غیرمستقیم از طریق `ServiceContracts` به `Entities` دسترسی دارد. این یعنی سرویس‌ها می‌توانند با موجودیت‌های دامنه کار کنند، اما لایه‌های بالاتر (مثل `CRUDExample`) به‌طور مستقیم به `Entities` ارجاع ندارند و فقط با DTO‌ها و اینترفیس‌ها کار می‌کنند. این جداسازی مسئولیت‌ها (Separation of Concerns) است.

---

## ۵. لایه‌ی Entities — مدل دامنه

این پروژه پایه‌ای‌ترین لایه است و شامل موجودیت‌های دامنه (Domain Entities) می‌شود. در پوشه‌ی ۰۸ فقط `Country` وجود دارد، اما در پوشه‌ی ۲۶ موجودیت `Person` هم اضافه شده است.

### ۵.۱ `Country.cs`

```csharp
namespace Entities
{
  /// <summary>
  /// Domain Model for Country
  /// </summary>
  public class Country
  {
    public Guid CountryID { get; set; }
    public string? CountryName { get; set; }
  }
}
```

**نکات طراحی:**
- `CountryID` از نوع `Guid` است که یک شناسه‌ی یکتای سراسری (Globally Unique Identifier) تولید می‌کند. این انتخاب از `int` مزیت‌هایی دارد: تخصیص غیرمتمرکز، عدم نیاز به هماهنگ‌سازی بین سرورها و مناسب برای محیط‌های توزیع‌شده.
- `CountryName` nullable است (`string?`) تا با قابلیت Nullable Reference Types در C# سازگار باشد و کامپایلر بتواند هشدارهای مربوط به null بودن را بدهد.
- این کلاس Domain Model محسوب می‌شود و نباید به DTO‌ها وابسته باشد.

### ۵.۲ `Person.cs` (فقط در پوشه‌ی ۲۶)

```csharp
namespace Entities
{
  /// <summary>
  /// Person domain model class
  /// </summary>
  public class Person
  {
    public Guid PersonID { get; set; }
    public string? PersonName { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public Guid? CountryID { get; set; }      // کلید خارجی به Country
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }
  }
}
```

**نکات طراحی:**
- `PersonID` از نوع `Guid` است (شناسه‌ی یکتای سراسری).
- `CountryID` یک کلید خارجی به موجودیت `Country` است. اما این به‌صورت soft reference است (یک فیلد Guid)، نه یک navigation property واقعی. در آینده وقتی EF Core اضافه شود، این می‌تواند به navigation property تبدیل شود.
- `Gender` به‌صورت `string?` ذخیره می‌شود نه enum. این یک تصمیم قابل بحث است (در پروژه‌های واقعی بهتر است از enum یا جدول lookup استفاده شود) ولی برای اهداف آموزشی این کفایت می‌کند. در DTO‌ها از `GenderOptions` enum استفاده می‌شود و در متد `ToPerson` با `.ToString()` به string تبدیل می‌شود.
- `ReceiveNewsLetters` یک `bool` non-nullable است که مقدار پیش‌فرض `false` دارد.
- تمام فیلدهای رشته‌ای `nullable` هستند تا با قابلیت Nullable Reference Types سازگار باشند.

---

## ۶. لایه‌ی ServiceContracts — قراردادها، DTO‌ها و Enum‌ها

این پروژه به‌طور قابل‌توجهی از پوشه‌ی ۰۸ به ۲۶ رشد کرده است. در پوشه‌ی ۰۸ فقط شامل ۲ DTO و ۱ اینترفیس است، اما در پوشه‌ی ۲۶ شامل ۵ DTO، ۲ enum و ۲ اینترفیس می‌باشد.

### ۶.۱ پوشه‌ی `Enums/` (فقط در پوشه‌ی ۲۶)

#### ۶.۱.۱ `GenderOptions.cs`

```csharp
public enum GenderOptions
{
  Male, Female, Other
}
```

#### ۶.۱.۲ `SortOrderOptions.cs`

```csharp
public enum SortOrderOptions
{
  ASC, DESC
}
```

این enum‌ها برای نوع‌داری (typing) قوی در DTO‌ها و پارامترهای متدها استفاده می‌شوند. مثلاً `PersonAddRequest.Gender` از نوع `GenderOptions?` است، و پارامتر `sortOrder` در `GetSortedPersons` از نوع `SortOrderOptions` است. این کار از خطاهای تایپی (مثل نوشتن `"ASC"` به‌جای `"AC"`) جلوگیری می‌کند و IDE می‌تواند Autocomplete را ارائه دهد.

### ۶.۲ پوشه‌ی `DTO/` — کلاس‌های انتقال داده

#### ۶.۲.۱ `CountryAddRequest.cs` (در هر دو پوشه — یکسان)

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

#### ۶.۲.۲ `CountryResponse.cs` (در هر دو پوشه — یکسان)

```csharp
public class CountryResponse
{
  public Guid CountryID { get; set; }
  public string? CountryName { get; set; }

  //It compares the current object to another object of CountryResponse type and returns true, if both values are same; otherwise returns false
  public override bool Equals(object? obj)
  {
    if (obj == null) return false;
    if (obj.GetType() != typeof(CountryResponse)) return false;
    CountryResponse country_to_compare = (CountryResponse)obj;
    return CountryID == country_to_compare.CountryID && CountryName == country_to_compare.CountryName;
  }

  public override int GetHashCode()
  {
    return base.GetHashCode();
  }
}

public static class CountryExtensions
{
  //Converts from Country object to CountryResponse object
  public static CountryResponse ToCountryResponse(this Country country)
  {
    return new CountryResponse() { CountryID = country.CountryID, CountryName = country.CountryName };
  }
}
```

**نکات طراحی:**
- `CountryResponse` DTO خروجی است؛ یعنی چیزی که سرویس به لایه‌های بالاتر (Controller یا Test) برمی‌گرداند.
- متد `Equals` override شده تا بتوان در تست‌ها با `Assert.Equal` دو شیء `CountryResponse` را با هم مقایسه کرد. این نکته در تست `GetCountryByCountryID_ValidCountryID` حیاتی است.
- کلاس استاتیک `CountryExtensions` یک Extension Method به نام `ToCountryResponse` تعریف می‌کند که هر شیء `Country` را به `CountryResponse` تبدیل می‌کند. این یک الگوی زیبا برای نگه‌داشتن منطق تبدیل نزدیک به نوع مقصد است.

#### ۶.۲.۳ `PersonAddRequest.cs` (فقط در پوشه‌ی ۲۶)

```csharp
public class PersonAddRequest
{
  [Required(ErrorMessage = "Person Name can't be blank")]
  public string? PersonName { get; set; }

  [Required(ErrorMessage = "Email can't be blank")]
  [EmailAddress(ErrorMessage = "Email value should be a valid email")]
  public string? Email { get; set; }

  public DateTime? DateOfBirth { get; set; }
  public GenderOptions? Gender { get; set; }
  public Guid? CountryID { get; set; }
  public string? Address { get; set; }
  public bool ReceiveNewsLetters { get; set; }

  public Person ToPerson()
  {
    return new Person() {
      PersonName = PersonName,
      Email = Email,
      DateOfBirth = DateOfBirth,
      Gender = Gender.ToString(),
      Address = Address,
      CountryID = CountryID,
      ReceiveNewsLetters = ReceiveNewsLetters
    };
  }
}
```

**نکات کلیدی:**
- استفاده از ویژگی‌های `System.ComponentModel.DataAnnotations` (`[Required]`, `[EmailAddress]`) برای اعلام اعتبارسنجی به‌صورت declarative. این ویژگی‌ها در `ValidationHelper` توسط `Validator.TryValidateObject` اجرا می‌شوند.
- متد `ToPerson()` یک متد تبدیل (Conversion) است که DTO را به مدل دامنه تبدیل می‌کند. نکته‌ی مهم: `Gender` که در DTO از نوع enum است، در مدل دامنه به `string` تبدیل می‌شود.

#### ۶.۲.۴ `PersonUpdateRequest.cs` (فقط در پوشه‌ی ۲۶)

```csharp
public class PersonUpdateRequest
{
  [Required(ErrorMessage = "Person ID can't be blank")]
  public Guid PersonID { get; set; }

  [Required(ErrorMessage = "Person Name can't be blank")]
  public string? PersonName { get; set; }

  [Required(ErrorMessage = "Email can't be blank")]
  [EmailAddress(ErrorMessage = "Email value should be a valid email")]
  public string? Email { get; set; }

  public DateTime? DateOfBirth { get; set; }
  public GenderOptions? Gender { get; set; }
  public Guid? CountryID { get; set; }
  public string? Address { get; set; }
  public bool ReceiveNewsLetters { get; set; }

  public Person ToPerson() { /* تبدیل DTO به مدل دامنه */ }
}
```

تفاوت با `PersonAddRequest`: فیلد `PersonID` اضافه شده تا مشخص کند کدام Person باید به‌روزرسانی شود. این یک الگوی استاندارد است: DTO‌های Add معمولاً ID ندارند (چون در سمت سرور تولید می‌شود)، اما DTO‌های Update حتماً ID دارند.

#### ۶.۲.۵ `PersonResponse.cs` (فقط در پوشه‌ی ۲۶)

```csharp
public class PersonResponse
{
  public Guid PersonID { get; set; }
  public string? PersonName { get; set; }
  public string? Email { get; set; }
  public DateTime? DateOfBirth { get; set; }
  public string? Gender { get; set; }
  public Guid? CountryID { get; set; }
  public string? Country { get; set; }                // نام کشور (lookup شده)
  public string? Address { get; set; }
  public bool ReceiveNewsLetters { get; set; }
  public double? Age { get; set; }                    // محاسبه‌شده از DateOfBirth

  public override bool Equals(object? obj) { /* مقایسه‌ی فیلدها */ }
  public override int GetHashCode() => base.GetHashCode();
  public override string ToString() { /* فرمت زیبا */ }

  public PersonUpdateRequest ToPersonUpdateRequest()
  {
    /* تبدیل Response به UpdateRequest برای استفاده در فرم Update */
  }
}

public static class PersonExtensions
{
  public static PersonResponse ToPersonResponse(this Person person)
  {
    return new PersonResponse() {
      PersonID = person.PersonID,
      PersonName = person.PersonName,
      Email = person.Email,
      DateOfBirth = person.DateOfBirth,
      ReceiveNewsLetters = person.ReceiveNewsLetters,
      Address = person.Address,
      CountryID = person.CountryID,
      Gender = person.Gender,
      Age = (person.DateOfBirth != null)
        ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25)
        : null
    };
  }
}
```

**نکات طراحی:**
- `Country` (نام کشور) در `PersonResponse` اضافه شده تا UI نیازی به lookup جداگانه نداشته باشد. این فیلد در `PersonsService.ConvertPersonToPersonResponse` پر می‌شود.
- `Age` یک فیلد محاسبه‌شده (Computed Field) است که در `ToPersonResponse` از روی `DateOfBirth` به دست می‌آید. تقسیم بر `365.25` برای لحاظ سال کبیسه است.
- متد `Equals` override شده تا در تست‌ها `Assert.Equal` بتواند دو `PersonResponse` را با مقایسه‌ی فیلدها (نه رفرنسی) مقایسه کند.
- متد `ToString` override شده تا خروجی تست‌ها در `ITestOutputHelper` خوانا باشد.
- متد `ToPersonUpdateRequest` مسیر برگشت از Response به UpdateRequest را فراهم می‌کند (مثلاً وقتی کاربر روی «Edit» کلیک می‌کند، داده‌ی موجود به فرم Update منتقل می‌شود).

### ۶.۳ `ICountriesService.cs` (در هر دو پوشه — یکسان)

```csharp
public interface ICountriesService
{
  CountryResponse AddCountry(CountryAddRequest? countryAddRequest);
  List<CountryResponse> GetAllCountries();
  CountryResponse? GetCountryByCountryID(Guid? countryID);  // ← محور پوشه‌ی ۰۸
}
```

این اینترفیس قرارداد (Contract) سرویس Countries را تعریف می‌کند. در پوشه‌ی ۰۸، محور توجه متد `GetCountryByCountryID` بود که تازه اضافه شده بود.

### ۶.۴ `IPersonsService.cs` (فقط در پوشه‌ی ۲۶)

```csharp
public interface IPersonsService
{
  PersonResponse AddPerson(PersonAddRequest? personAddRequest);
  List<PersonResponse> GetAllPersons();
  PersonResponse? GetPersonByPersonID(Guid? personID);
  List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString);
  List<PersonResponse> GetSortedPersons(
    List<PersonResponse> allPersons,
    string sortBy,
    SortOrderOptions sortOrder);
  PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest);
  bool DeletePerson(Guid? personID);                  // ← محور پوشه‌ی ۲۶
}
```

این اینترفیس قرارداد کامل CRUD برای Person را تعریف می‌کند. در پوشه‌ی ۲۶، محور توجه متد `DeletePerson` بود که تازه اضافه و پیاده‌سازی شده بود.

| متد                  | ورودی                                  | خروجی                    | توضیح                                       |
|----------------------|----------------------------------------|--------------------------|---------------------------------------------|
| `AddPerson`          | `PersonAddRequest?`                    | `PersonResponse`         | افزودن شخص جدید                             |
| `GetAllPersons`      | —                                      | `List<PersonResponse>`   | برگرداندن همه‌ی اشخاص                       |
| `GetPersonByPersonID`| `Guid? personID`                       | `PersonResponse?`        | پیدا کردن شخص با ID مشخص                    |
| `GetFilteredPersons` | `string searchBy`, `string? searchString` | `List<PersonResponse>` | فیلتر کردن اشخاص                            |
| `GetSortedPersons`   | `List<PersonResponse>`, `string sortBy`, `SortOrderOptions sortOrder` | `List<PersonResponse>` | مرتب‌سازی اشخاص |
| `UpdatePerson`       | `PersonUpdateRequest?`                 | `PersonResponse`         | به‌روزرسانی اشخاص                           |
| `DeletePerson`       | `Guid? personID`                       | `bool`                   | حذف شخص — **محور پوشه‌ی ۲۶**                 |

---

## ۷. لایه‌ی Services — پیاده‌سازی سرویس‌ها

این لایه پیاده‌سازی واقعی منطق کسب‌وکار را در خود نگه می‌دارد. در پوشه‌ی ۰۸ فقط `CountriesService` وجود دارد، اما در پوشه‌ی ۲۶، `PersonsService` و `ValidationHelper` هم اضافه شده‌اند.

### ۷.۱ `CountriesService.cs` (در هر دو پوشه — یکسان)

```csharp
public class CountriesService : ICountriesService
{
  private readonly List<Country> _countries;

  public CountriesService()
  {
    _countries = new List<Country>();
  }

  public CountryResponse AddCountry(CountryAddRequest? countryAddRequest) { /* ... */ }
  public List<CountryResponse> GetAllCountries() { /* ... */ }
  public CountryResponse? GetCountryByCountryID(Guid? countryID) { /* ... */ }
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

4. **پیاده‌سازی `GetCountryByCountryID` (محور پوشه‌ی ۰۸):**
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

### ۷.۲ `Helpers/ValidationHelper.cs` (فقط در پوشه‌ی ۲۶)

```csharp
public class ValidationHelper
{
  internal static void ModelValidation(object obj)
  {
    ValidationContext validationContext = new ValidationContext(obj);
    List<ValidationResult> validationResults = new List<ValidationResult>();

    bool isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);
    if (!isValid)
    {
      throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage);
    }
  }
}
```

**نقش:** این کلاس استاتیک، اعتبارسنجی declarative را اجرا می‌کند. وقتی یک شیء DTO (مثل `PersonAddRequest` یا `PersonUpdateRequest`) به آن داده می‌شود، تمام ویژگی‌های `DataAnnotations` (مثل `[Required]`, `[EmailAddress]`) را بررسی می‌کند و اگر معتبر نباشد، `ArgumentException` با اولین پیام خطا پرتاب می‌کند.

**نکته‌ی مهم:** متد `internal static` است، یعنی فقط از داخل همان assembly (پروژه‌ی `Services`) قابل دسترسی است. این یک تصمیم طراحی است تا لایه‌های خارجی نتوانند مستقیماً از آن استفاده کنند و فقط سرویس‌ها بتوانند از آن بهره ببرند.

### ۷.۳ `PersonsService.cs` (فقط در پوشه‌ی ۲۶)

```csharp
public class PersonsService : IPersonsService
{
  private readonly List<Person> _persons;
  private readonly ICountriesService _countriesService;

  public PersonsService()
  {
    _persons = new List<Person>();
    _countriesService = new CountriesService();    // ← تزریق وابستگی به‌صورت دستی
  }

  private PersonResponse ConvertPersonToPersonResponse(Person person)
  {
    PersonResponse personResponse = person.ToPersonResponse();
    personResponse.Country =
      _countriesService.GetCountryByCountryID(person.CountryID)?.CountryName;
    return personResponse;
  }

  // متدهای AddPerson, GetAllPersons, GetPersonByPersonID,
  // GetFilteredPersons, GetSortedPersons, UpdatePerson, DeletePerson
}
```

**نکات طراحی:**

1. **دو وابستگی:** این سرویس به دو لیست/سرویس وابسته است:
   - `_persons`: لیست in-memory از اشخاص
   - `_countriesService`: نمونه‌ای از `CountriesService` برای lookup نام کشور

2. **تزریق دستی (Manual Injection):** فعلاً `CountriesService` در سازنده به‌صورت دستی `new` می‌شود. این کار در آینده با Dependency Injection و Moq جایگزین می‌شود.

3. **متد کمکی `ConvertPersonToPersonResponse`:** این متد private، ابتدا `Person` را با `ToPersonResponse()` به DTO تبدیل می‌کند، سپس نام کشور را با فراخوانی `GetCountryByCountryID` از سرویس کشور پر می‌کند. این الگو Composition است (سرویس Person از سرویس Country استفاده می‌کند).

#### ۷.۳.۱ `AddPerson`

- اگر `personAddRequest` برابر `null` باشد → `ArgumentNullException`
- اعتبارسنجی مدل با `ValidationHelper.ModelValidation` (بررسی `[Required]` و `[EmailAddress]`)
- تبدیل DTO به `Person` با `ToPerson()`
- تولید `PersonID` با `Guid.NewGuid()`
- افزودن به لیست `_persons`
- بازگرداندن `PersonResponse` با پر کردن نام کشور از طریق `ConvertPersonToPersonResponse`

#### ۷.۳.۲ `GetAllPersons`

تبدیل همه‌ی اشخاص به `PersonResponse` و برگرداندن لیست.

#### ۷.۳.۳ `GetPersonByPersonID`

- اگر `personID` برابر `null` باشد → `null`
- جستجو با LINQ `FirstOrDefault`
- اگر پیدا نشد → `null`
- در غیر این صورت → تبدیل به `PersonResponse`

#### ۷.۳.۴ `GetFilteredPersons`

- اگر `searchBy` یا `searchString` خالی باشد → همه‌ی اشخاص برگردانده می‌شوند
- در غیر این صورت با `switch` روی `searchBy`:
  - `PersonName` → جستجوی `Contains` (case-insensitive)
  - `Email` → جستجوی `Contains`
  - `DateOfBirth` → تبدیل به رشته‌ی "dd MMMM yyyy" و سپس جستجو
  - `Gender` → جستجوی `Contains`
  - `CountryID` → جستجو در `Country` (نام کشور)
  - `Address` → جستجوی `Contains`
  - default → همه‌ی اشخاص

#### ۷.۳.۵ `GetSortedPersons` (با Switch Expression)

```csharp
public List<PersonResponse> GetSortedPersons(
  List<PersonResponse> allPersons,
  string sortBy,
  SortOrderOptions sortOrder)
{
  if (string.IsNullOrEmpty(sortBy))
    return allPersons;

  List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
  {
    (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) =>
      allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
    (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) =>
      allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
    // ... ۱۴ حالت دیگر برای Email, DateOfBirth, Age, Gender, Country, Address, ReceiveNewsLetters
    _ => allPersons    // default
  };

  return sortedPersons;
}
```

این متد از **Switch Expression با Tuple Pattern** استفاده می‌کند که همزمان دو متغیر `(sortBy, sortOrder)` را بررسی می‌کند. این الگو کد را فشرده، خوانا و قابل نگه‌داری می‌کند. در مجموع ۱۶ حالت (۲ حالت ASC/DESC برای ۸ فیلد) در switch وجود دارد.

#### ۷.۳.۶ `UpdatePerson`

- اگر `personUpdateRequest` برابر `null` باشد → `ArgumentNullException`
- اعتبارسنجی مدل با `ValidationHelper.ModelValidation`
- جستجوی شخص با `PersonID`
- اگر پیدا نشد → `ArgumentException("Given person id doesn't exist")`
- به‌روزرسانی همه‌ی فیلدها روی شخص پیدا‌شده
- بازگرداندن `PersonResponse`

#### ۷.۳.۷ `DeletePerson` (محور پوشه‌ی ۲۶)

```csharp
public bool DeletePerson(Guid? personID)
{
  if (personID == null)
  {
    throw new ArgumentNullException(nameof(personID));
  }

  Person? person = _persons.FirstOrDefault(temp => temp.PersonID == personID);
  if (person == null)
    return false;

  _persons.RemoveAll(temp => temp.PersonID == personID);

  return true;
}
```

**تحلیل:**
1. اگر `personID` برابر `null` باشد → `ArgumentNullException` پرتاب می‌شود. این رفتار با متدهای دیگر (`GetPersonByPersonID`) متفاوت است که در آن null می‌دادیم و `null` برمی‌گرفت. دلیل تفاوت: حذف یک عملیات مخرب (Destructive) است و بهتر است صراحتاً به caller خطا دهد تا به‌طور خاموش `false` برگرداند.
2. ابتدا با `FirstOrDefault` بررسی می‌کند که آیا شخص با این ID وجود دارد یا نه.
3. اگر وجود نداشت → `false` برمی‌گرداند.
4. اگر وجود داشت → با `RemoveAll` آن را از لیست حذف می‌کند و `true` برمی‌گرداند.
5. **نکته‌ی بهینه‌سازی:** چون `person` پیدا شده، می‌توان مستقیماً `_persons.Remove(person)` کرد. استفاده از `RemoveAll` برای اطمینان از حذف همه‌ی موارد منطبق (در حالت تئوریک چندتایی) است.

---

## ۸. لایه‌ی CRUDExample — وب اپلیکیشن

این پروژه میزبان (Host) اجرایی است که اپلیکیشن را بالا می‌آورد. در هر دو پوشه، کد Program.cs یکسان است.

### ۸.۱ `Program.cs` (Minimal Hosting Model — در هر دو پوشه یکسان)

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
- **نکته‌ی مهم:** در این مرحله از دوره، هنوز `ICountriesService` و `IPersonsService` در DI Container رجیستر نشده‌اند؛ این کار در قسمت بعدی انجام خواهد شد. به همین دلیل فعلاً در `Program.cs` ارجاعی به پروژه‌ی `Services` وجود ندارد.

### ۸.۲ سایر فایل‌ها

- `appsettings.json` و `appsettings.Development.json`: تنظیمات Logging
- `Properties/launchSettings.json`: اپلیکیشن روی `http://localhost:5298` اجرا می‌شود
- `wwwroot/StyleSheet.css`: فایل CSS استاتیک

---

## ۹. لایه‌ی CRUDTests — تست‌های واحد

این پروژه شامل تمام تست‌های واحد است که رفتار سرویس‌ها را می‌سنجد. در پوشه‌ی ۰۸ فقط `CountriesServiceTest` است، اما در پوشه‌ی ۲۶، `PersonsServiceTest` هم اضافه شده است.

### ۹.۱ `CountriesServiceTest.cs` (در هر دو پوشه — یکسان)

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

**ج) `GetCountryByCountryID` (۲ تست — محور پوشه‌ی ۰۸):**

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

### ۹.۲ `PersonsServiceTest.cs` (فقط در پوشه‌ی ۲۶)

این کلاس تست بسیار کامل‌تر و بزرگ‌تر از `CountriesServiceTest` است. ساختار کلی آن به این شکل:

```csharp
public class PersonsServiceTest
{
  private readonly IPersonsService _personService;
  private readonly ICountriesService _countriesService;
  private readonly ITestOutputHelper _testOutputHelper;

  public PersonsServiceTest(ITestOutputHelper testOutputHelper)
  {
    _personService = new PersonsService();
    _countriesService = new CountriesService();
    _testOutputHelper = testOutputHelper;
  }

  // ... ۷ region از تست‌ها
}
```

**نکات طراحی:**
1. **دو سرویس تست می‌شوند:** `IPersonsService` و `ICountriesService`. دلیل: بسیاری از تست‌های Person نیاز به یک Country معتبر دارند (چون `CountryID` کلید خارجی است).
2. **استفاده از `ITestOutputHelper`:** این اینترفیس از xUnit به تست اجازه می‌دهد خروجی را به کنسول تست بنویسد. در تست‌های `GetAllPersons`، `GetFilteredPersons` و `GetSortedPersons`، لیست مورد انتظار و لیست واقعی چاپ می‌شوند تا دیباگ راحت‌تر باشد.
3. **الگوی AAA:** تمام تست‌ها از الگوی Arrange-Act-Assert پیروی می‌کنند.

#### تست‌های موجود در `PersonsServiceTest`:

این کلاس به هفت `#region` تقسیم شده:

**الف) `AddPerson` (۳ تست):**
- `AddPerson_NullPerson` — اگر ورودی `null` باشد، باید `ArgumentNullException` پرتاب شود
- `AddPerson_PersonNameIsNull` — اگر `PersonName` برابر `null` باشد، باید `ArgumentException` پرتاب شود (به‌خاطر `[Required]`)
- `AddPerson_ProperPersonDetails` — در حالت درست، شخص باید با `PersonID` غیرخالی به لیست اضافه شود

**ب) `GetPersonByPersonID` (۲ تست):**
- `GetPersonByPersonID_NullPersonID` — ورودی `null` → خروجی `null`
- `GetPersonByPersonID_WithPersonID` — با ID معتبر، باید همان شخص برگردانده شود

**ج) `GetAllPersons` (۲ تست):**
- `GetAllPersons_EmptyList` — به‌طور پیش‌فرض لیست باید خالی باشد
- `GetAllPersons_AddFewPersons` — وقتی چند شخص اضافه می‌شود، باید در لیست بازگشتی موجود باشند

**د) `GetFilteredPersons` (۲ تست):**
- `GetFilteredPersons_EmptySearchText` — وقتی `searchString` خالی است، همه باید برگردانده شوند
- `GetFilteredPersons_SearchByPersonName` — جستجوی "ma" باید اشخاص منطبق (مثل "Mary" و "Rahman") برگرداند

**هـ) `GetSortedPersons` (۱ تست):**
- `GetSortedPersons` — مرتب‌سازی DESC روی `PersonName` باید ترتیب نزولی تولید کند

**و) `UpdatePerson` (۴ تست):**
- `UpdatePerson_NullPerson` — ورودی `null` → `ArgumentNullException`
- `UpdatePerson_InvalidPersonID` — ID نامعتبر → `ArgumentException`
- `UpdatePerson_PersonNameIsNull` — `PersonName` برابر `null` → `ArgumentException`
- `UpdatePerson_PersonFullDetailsUpdation` — به‌روزرسانی کامل باید موفق باشد و شخص به‌روزرسانی‌شده با خروجی `GetPersonByPersonID` منطبق باشد

**ز) `DeletePerson` (۲ تست — محور پوشه‌ی ۲۶):**

```csharp
[Fact]
public void DeletePerson_ValidPersonID()
{
  //Arrange
  CountryAddRequest country_add_request =
    new CountryAddRequest() { CountryName = "USA" };
  CountryResponse country_response_from_add =
    _countriesService.AddCountry(country_add_request);

  PersonAddRequest person_add_request = new PersonAddRequest() {
    PersonName = "Jones",
    Address = "address",
    CountryID = country_response_from_add.CountryID,
    DateOfBirth = Convert.ToDateTime("2010-01-01"),
    Email = "jones@example.com",
    Gender = GenderOptions.Male,
    ReceiveNewsLetters = true
  };

  PersonResponse person_response_from_add =
    _personService.AddPerson(person_add_request);

  //Act
  bool isDeleted = _personService.DeletePerson(person_response_from_add.PersonID);

  //Assert
  Assert.True(isDeleted);
}

[Fact]
public void DeletePerson_InvalidPersonID()
{
  //Act
  bool isDeleted = _personService.DeletePerson(Guid.NewGuid());

  //Assert
  Assert.False(isDeleted);
}
```

**تحلیل تست‌های `DeletePerson`:**

1. **تست ValidPersonID:**
   - **Arrange:** ابتدا یک کشور با نام "USA" اضافه می‌کند تا یک `CountryResponse` معتبر داشته باشد. سپس یک شخص با تمام فیلدها (شامل `CountryID` معتبر) اضافه می‌کند.
   - **Act:** سپس متد `DeletePerson` را با `PersonID` همان شخص فراخوانی می‌کند.
   - **Assert:** نتیجه باید `true` باشد. این تست بررسی می‌کند که وقتی ID معتبر است، حذف موفق است.

2. **تست InvalidPersonID:**
   - **Act:** متد `DeletePerson` را با یک `Guid.NewGuid()` تصادفی (که قطعاً در لیست وجود ندارد) فراخوانی می‌کند.
   - **Assert:** نتیجه باید `false` باشد. این تست بررسی می‌کند که وقتی ID نامعتبر است، متد به‌جای پرتاب exception، `false` برمی‌گرداند.

> **نکته‌ی طراحی تست:** هر دو تست روی یک سناریوی خالص تمرکز دارند — یکی حالت موفق و دیگری حالت ناموفق. این رویکرد «تست‌های تمامیت‌پذیری» (Happy Path + Sad Path) رفتار متد را به‌طور کامل پوشش می‌دهد. توجه کنید که رفتار `null` بودن ورودی به‌طور جداگانه توسط منطق `ArgumentNullException` در خود متد پوشش داده شده، اما در تست‌ها صریحاً تست نشده است (این یک فرصت بهبود برای خواننده است).

### ۹.۳ سایر فایل‌های تست

- **`UnitTest1.cs` و `MyMath.cs`:** این دو فایل یک تست نمونه‌ی ابتدایی هستند که در ابتدای دوره نوشته شده‌اند تا مفهوم xUnit را ساده توضیح دهند. کلاس `MyMath` یک متد `Add` ساده دارد که دو عدد را جمع می‌کند. این فایل‌ها ارتباط مستقیمی با منطق کشور یا شخص ندارند.

---

## ۱۰. نمودار ارتباط کامل کلاس‌ها و سرویس‌ها

این نمودار، ارتباط کامل تمام کلاس‌ها و سرویس‌ها را در پوشه‌ی ۲۶ (که شامل Person هم می‌شود) نشان می‌دهد:

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│                              CRUDTests (Tests)                              │
│                                                                              │
│   PersonsServiceTest (۲۶)                                                   │
│   ├── depends on → IPersonsService (Contract)                              │
│   ├── depends on → ICountriesService (Contract)                            │
│   ├── instantiation → new PersonsService(), new CountriesService()         │
│   ├── uses DTO: PersonAddRequest, PersonUpdateRequest, PersonResponse     │
│   ├── uses DTO: CountryAddRequest, CountryResponse                          │
│   ├── uses Enum: GenderOptions, SortOrderOptions                          │
│   └── uses ITestOutputHelper (xUnit) for logging                            │
│                                                                              │
│   CountriesServiceTest (در هر دو پوشه)                                       │
│   └── depends on → ICountriesService (Contract)                            │
│                                                                              │
│   UnitTest1 → MyMath (تست نمونه‌ی ابتدایی)                                  │
└──────────────────────────────────────────────────────────────────────────────┘
                                  │
                                  │ uses (polymorphism)
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                       ServiceContracts (Contracts)                          │
│                                                                              │
│   ┌─────────────────────┐       ┌──────────────────────┐                     │
│   │ ICountriesService   │       │ IPersonsService (۲۶) │                    │
│   │ - AddCountry()      │       │ - AddPerson()        │                    │
│   │ - GetAllCountries() │       │ - GetAllPersons()    │                    │
│   │ - GetCountryBy      │       │ - GetPersonByPersonID│                    │
│   │   CountryID()       │       │ - GetFilteredPersons │                    │
│   └─────────┬───────────┘       │ - GetSortedPersons   │                    │
│             │ implements         │ - UpdatePerson       │                    │
│             │                    │ - DeletePerson       │ ← محور پوشه‌ی ۲۶  │
│             │                    └──────────┬───────────┘                    │
│             │                               │ implements                    │
│             │                               │                               │
│   ┌─────────┴───────────────────────────────┴────────────┐                 │
│   │                    DTO Folder                          │                 │
│   │  ├── CountryAddRequest  ├── CountryResponse           │                 │
│   │  │   └── ToCountry()     │   ├── Equals()             │                 │
│   │  │                       │   └── CountryExtensions    │                 │
│   │  │                       │       └── ToCountryResponse│                 │
│   │  ├── PersonAddRequest (۲۶)│                            │                 │
│   │  │   ├── [Required] attrs │                            │                 │
│   │  │   └── ToPerson()       │                            │                 │
│   │  ├── PersonUpdateRequest (۲۶)│                         │                 │
│   │  │   ├── [Required] attrs │                            │                 │
│   │  │   └── ToPerson()       │                            │                 │
│   │  └── PersonResponse (۲۶)  │                            │                 │
│   │      ├── Equals()         │                            │                 │
│   │      ├── ToString()       │                            │                 │
│   │      ├── ToPersonUpdateRequest()                       │                 │
│   │      └── PersonExtensions.ToPersonResponse()          │                 │
│   └──────────────────────────────────────────────────────┘                  │
│                                                                              │
│   ┌──────────────────────────────────────────────────────┐                  │
│   │              Enums Folder (۲۶)                       │                  │
│   │   ├── GenderOptions (Male, Female, Other)            │                  │
│   │   └── SortOrderOptions (ASC, DESC)                   │                  │
│   └──────────────────────────────────────────────────────┘                  │
└──────────────────────────────────────────────────────────────────────────────┘
                                  │
                                  │ implements
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                        Services (Implementation)                            │
│                                                                              │
│   PersonsService : IPersonsService (۲۶)                                     │
│   ├── private fields:                                                       │
│   │   ├── List<Person> _persons (in-memory)                                 │
│   │   └── ICountriesService _countriesService (composition)                │
│   ├── constructor:                                                          │
│   │   ├── new List<Person>()                                                │
│   │   └── new CountriesService()  ← Manual injection                        │
│   ├── private helper: ConvertPersonToPersonResponse(person)                 │
│   │   └── fills Country name via _countriesService.GetCountryByCountryID   │
│   ├── AddPerson() → ValidationHelper → ToPerson() → add → Convert         │
│   ├── GetAllPersons() → Select(ToPersonResponse)                            │
│   ├── GetPersonByPersonID() → FirstOrDefault → ToPersonResponse             │
│   ├── GetFilteredPersons() → switch on searchBy → Where(...)                │
│   ├── GetSortedPersons() → 16-case switch expression → OrderBy/Descending  │
│   ├── UpdatePerson() → ValidationHelper → FirstOrDefault → update fields   │
│   └── DeletePerson() → FirstOrDefault → RemoveAll → true/false             │
│                                                                              │
│   CountriesService : ICountriesService (در هر دو پوشه)                       │
│   └── (مانند شرح بالا)                                                       │
│                                                                              │
│   Helpers/ValidationHelper (۲۶)                                             │
│   └── internal static ModelValidation(object)                               │
│       └── uses Validator.TryValidateObject (DataAnnotations)              │
└──────────────────────────────────────────────────────────────────────────────┘
                                  │
                                  │ uses
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                          Entities (Domain)                                  │
│                                                                              │
│   Country (در هر دو پوشه)                                                    │
│   ├── Guid  CountryID                                                        │
│   └── string? CountryName                                                   │
│                                                                              │
│   Person (۲۶)                                                                │
│   ├── Guid  PersonID                                                         │
│   ├── string? PersonName                                                    │
│   ├── string? Email                                                          │
│   ├── DateTime? DateOfBirth                                                  │
│   ├── string? Gender                  (ذخیره به‌صورت string، نه enum)       │
│   ├── Guid? CountryID                 (FK به Country)                         │
│   ├── string? Address                                                        │
│   └── bool   ReceiveNewsLetters                                              │
└──────────────────────────────────────────────────────────────────────────────┘
                                  ▲
                                  │ hosts (but not yet wired)
                                  │
┌──────────────────────────────────────────────────────────────────────────────┐
│                         CRUDExample (Web Host)                              │
│                                                                              │
│   Program.cs (Minimal Hosting — در هر دو پوشه یکسان)                          │
│   ├── AddControllersWithViews()  (MVC registered)                           │
│   ├── UseStaticFiles()                                                       │
│   ├── MapControllers()                                                       │
│   └── (Registration of services will be added in subsequent sections)       │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## ۱۱. جریان اجرای متدهای محوری

### ۱۱.۱ جریان اجرای `GetCountryByCountryID` (محور پوشه‌ی ۰۸)

برای درک بهتر ارتباط کلاس‌ها، جریان اجرای یک فراخوانی کامل به این متد را مرور می‌کنیم:

```text
1. فراخواننده (Test یا Controller)
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

### ۱۱.۲ جریان اجرای `DeletePerson` (محور پوشه‌ی ۲۶)

برای درک بهتر ارتباط کلاس‌ها، جریان اجرای یک فراخوانی کامل به این متد را مرور می‌کنیم:

```text
1. فراخواننده (Test یا Controller)
   │
   │  DeletePerson(Guid? personID)
   ▼
2. IPersonsService.DeletePerson(personID)               [Contract]
   │  (polymorphic dispatch)
   ▼
3. PersonsService.DeletePerson(personID)                [Implementation]
   │
   ├── 3.1. بررسی null بودن personID
   │        └── اگر null → throw ArgumentNullException(nameof(personID))
   │
   ├── 3.2. LINQ: _persons.FirstOrDefault(p => p.PersonID == personID)
   │        └── در لیست in-memory جستجو می‌کند
   │        └── روی Entity از نوع Person کار می‌کند
   │
   ├── 3.3. اگر person برابر null (یعنی پیدا نشد)
   │        └── return false
   │
   └── 3.4. _persons.RemoveAll(p => p.PersonID == personID)
            └── حذف تمام موارد منطبق از لیست
            ▼
            return true
   ▼
4. bool به فراخواننده برگردانده می‌شود
   │
   │  (در تست DeletePerson_ValidPersonID: Assert.True)
   │  (در تست DeletePerson_InvalidPersonID: Assert.False)
   ▼
5. تست با Assert.True یا Assert.False نتیجه را می‌سنجد
```

---

## ۱۲. مفاهیم و الگوهای طراحی به‌کار رفته

این پروژه‌ها، نمونه‌ی کلاسیک و تمیزی از چند الگوی طراحی و معماری است:

### ۱۲.۱ معماری لایه‌ای (Layered Architecture)

پروژه به چهار لایه‌ی مجزا تقسیم شده:

1. **لایه‌ی دامنه (Domain Layer):** `Entities`
2. **لایه‌ی قراردادها (Contract Layer):** `ServiceContracts` (شامل DTO‌ها و Enum‌ها)
3. **لایه‌ی پیاده‌سازی (Implementation Layer):** `Services` (شامل Helpers)
4. **لایه‌ی میزبان (Host Layer):** `CRUDExample`
5. **لایه‌ی تست (Test Layer):** `CRUDTests`

مزایای این الگو:
- جداسازی واضح مسئولیت‌ها (Separation of Concerns)
- قابلیت تست‌پذیری بالا (می‌توان پیاده‌سازی سرویس را با Mock جایگزین کرد)
- قابل تعویض بودن پیاده‌سازی (مثلاً جایگزینی ذخیره‌سازی in-memory با EF Core)
- جلوگیری از Circular Dependency

### ۱۲.۲ الگوی DTO (Data Transfer Object)

پروژه برای هر موجودیت دو یا سه DTO دارد:
- **`CountryAddRequest`** برای ورودی Add Country
- **`CountryResponse`** برای خروجی Country
- **`PersonAddRequest`** برای ورودی Add Person
- **`PersonUpdateRequest`** برای ورودی Update Person
- **`PersonResponse`** برای خروجی همه‌ی متدهای Person

مزایا:
- پنهان کردن جزئیات داخلی مدل دامنه (Encapsulation)
- امکان تغییر مدل دامنه بدون شکستن API
- جداسازی اعتبارسنجی ورودی از منطق دامنه
- کنترل دقیق روی فیلدهای قابل ارسال

### ۱۲.۳ الگوی Interface-Based Design (Program to Interface)

سرویس‌ها با اینترفیس (`ICountriesService`, `IPersonsService`) تعریف می‌شوند و پیاده‌سازی (`CountriesService`, `PersonsService`) جداگانه. تست‌ها هم به اینترفیس ارجاع می‌دهند نه پیاده‌سازی مستقیم. این کار:
- امکان تزریق وابستگی (Dependency Injection) را فراهم می‌کند
- جایگزینی پیاده‌سازی با Mock (در آینده با Moq) را ممکن می‌سازد
- قرارداد روشنی بین لایه‌ها ایجاد می‌کند

### ۱۲.۴ Extension Methods

استفاده از `CountryExtensions.ToCountryResponse` و `PersonExtensions.ToPersonResponse` برای تبدیل، یک الگوی تمیز برای:
- نگه‌داشتن منطق تبدیل نزدیک به نوع مقصد
- کدخوانایی بالاتر (مثل `country.ToCountryResponse()`)
- قابلیت استفاده‌ی مجدد در سراسر کد

### ۱۲.۵ الگوی AAA در تست‌ها

تمام تست‌های xUnit از الگوی Arrange-Act-Assert پیروی می‌کنند که کد تست را خوانا و قابل نگه‌داری می‌کند.

### ۱۲.۶ In-Memory Storage (موقت)

پیاده‌سازی فعلی از `List<Person>` و `List<Country>` به‌عنوان ذخیره‌ساز استفاده می‌کند. این انتخاب برای اهداف آموزشی است و در قسمت‌های بعدی دوره با EntityFrameworkCore جایگزین خواهد شد.

### ۱۲.۷ Composition بین سرویس‌ها (در پوشه‌ی ۲۶)

`PersonsService` به `ICountriesService` وابسته است تا نام کشور را برای `PersonResponse` پر کند. این یک الگوی Composition است: سرویس‌ها با هم همکاری می‌کنند. اما فعلاً این Composition به‌صورت دستی (`new CountriesService()`) انجام می‌شود که در آینده با DI جایگزین می‌شود.

### ۱۲.۸ Declarative Validation با DataAnnotations (در پوشه‌ی ۲۶)

استفاده از ویژگی‌های `[Required]` و `[EmailAddress]` در DTO‌ها به‌صورت declarative و سپس اجرای آن‌ها با `ValidationHelper.ModelValidation` یک الگوی رایج در ASP.NET Core است. این کار:
- منطق اعتبارسنجی را به‌صورت Attribute روی فیلدها می‌نویسد
- با یک متد عمومی در `ValidationHelper` اجرا می‌شود
- قابل استفاده مجدد برای همه‌ی DTO‌هاست

### ۱۲.۹ Pattern Matching در C# (در پوشه‌ی ۲۶)

استفاده از Switch Expression با Pattern Matching در `GetSortedPersons`:

```csharp
List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
{
  (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) => allPersons.OrderBy(...).ToList(),
  (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) => allPersons.OrderByDescending(...).ToList(),
  // ... 14 حالت دیگر
  _ => allPersons
};
```

این الگو کد را فشرده، خوانا و قابل نگه‌داری می‌کند. Switch Expression مقدار برمی‌گرداند و می‌توان آن را مستقیم به متغیر نسبت داد.

### ۱۲.۱۰ Computed Fields در DTO (در پوشه‌ی ۲۶)

فیلد `Age` در `PersonResponse` به‌صورت خودکار از روی `DateOfBirth` محاسبه می‌شود (در `ToPersonResponse`). این کار:
- یک فیلد مفهومی (نه ذخیره‌سازی‌شده) را به UI عرضه می‌کند
- نیازی به ذخیره‌سازی جداگانه ندارد
- هر بار به‌روز می‌شود

---

## ۱۳. مقایسه‌ی پوشه‌ی ۰۸ با پوشه‌ی ۲۶

این مقایسه نشان می‌دهد که در طی ۱۸ قسمت (۰۸ تا ۲۶)، پروژه از یک سرویس ساده با ۱ موجودیت به یک اپلیکیشن CRUD کامل با ۲ موجودیت، ۷ عملیات و ۱۵ تست واحد رشد کرده است.

| جنبه                         | پوشه‌ی ۰۸                                       | پوشه‌ی ۲۶                                            |
|------------------------------|------------------------------------------------|----------------------------------------------------|
| **تعداد موجودیت‌ها**          | ۱ (Country)                                    | ۲ (Country + Person)                              |
| **تعداد DTO‌ها**              | ۲ (CountryAddRequest, CountryResponse)         | ۵ (۲ Country + ۳ Person)                         |
| **تعداد سرویس‌ها**            | ۱ (CountriesService)                           | ۲ (CountriesService + PersonsService)            |
| **تعداد اینترفیس‌ها**         | ۱ (ICountriesService)                          | ۲ (ICountriesService + IPersonsService)           |
| **Enum‌ها**                  | ندارد                                          | ۲ (GenderOptions, SortOrderOptions)               |
| **Helperها**                 | ندارد                                          | ۱ (ValidationHelper)                              |
| **DataAnnotations**          | ندارد                                          | فعال ([Required], [EmailAddress])                 |
| **متد محور**                 | `GetCountryByCountryID`                        | `DeletePerson`                                    |
| **تست‌های محور**             | ۲ (NullCountryID, ValidCountryID)              | ۲ (ValidPersonID, InvalidPersonID)               |
| **تعداد کل تست‌های Person**  | —                                              | ۱۵ (AddPerson ۳ + Get ۲ + GetAll ۲ + Filter ۲ + Sort ۱ + Update ۴ + Delete ۲) |
| **Composition بین سرویس‌ها** | ندارد                                          | PersonsService → CountriesService                 |
| **Pattern Matching**         | ندارد                                          | در GetSortedPersons                               |
| **ITestOutputHelper**        | استفاده نشده                                   | در PersonsServiceTest                             |
| **تعداد کل فایل‌های سورس**   | ۸                                              | ۱۳                                                |

---

## ۱۴. نکات قابل توجه و فرصت‌های بهبود

### ۱۴.۱ نکات مشترک هر دو پوشه

1. **هنوز DI فعال نیست:** در `Program.cs` پروژه‌ی `CRUDExample`، `ICountriesService` (و `IPersonsService` در پوشه‌ی ۲۶) رجیستر نشده است. این کار در قسمت بعدی انجام می‌شود.

2. **تست‌ها به‌جای DI، مستقیماً new می‌کنند:** در سازنده‌ی `CountriesServiceTest` (و `PersonsServiceTest`)، `new CountriesService()` (و `new PersonsService()`) فراخوانی می‌شود. این الگو وقتی منطق ساده است و وابستگی خارجی ندارد، قابل قبول است. در آینده با Moq این الگو تغییر می‌کند.

3. **`GetHashCode` به‌سادگی override شده:** در هر دو `CountryResponse` و `PersonResponse`، این متد به `base.GetHashCode()` ارجاع می‌دهد که در عمل یک انتخاب نه‌چندان اصولی است (وقتی `Equals` override می‌شود، `GetHashCode` هم باید منطق متناظر داشته باشد). این نکته در پروژه‌های واقعی باید اصلاح شود.

### ۱۴.۲ نکات مربوط به پوشه‌ی ۰۸

4. **هدف override شدن `Equals` در `CountryResponse`:** این کار فقط برای امکان مقایسه در `Assert.Equal` انجام شده است. بدون این override، مقایسه‌ی رفرنسی انجام می‌شد و تست شکست می‌خورد.

5. **بهینه‌سازی احتمالی:** در `AddCountry`، چک تکراری بودن نام کشور با `Where(...).Count() > 0` نوشته شده. استفاده از `Any(...)` کارآمدتر است چون به‌محض پیدا شدن اولین مورد متوقف می‌شود.

### ۱۴.۳ نکات مربوط به پوشه‌ی ۲۶

6. **رشد قابل‌توجه پروژه:** نسبت به پوشه‌ی ۰۸، این پوشه کلاس‌های بسیار بیشتری دارد: ۲ موجودیت، ۵ DTO، ۲ enum، ۲ سرویس، ۱ helper و ۲ کلاس تست. این نشان می‌دهد پروژه در حال نزدیک شدن به یک اپلیکیشن واقعی است.

7. **تزریق دستی در PersonsService:** در سازنده‌ی `PersonsService`، `new CountriesService()` فراخوانی می‌شود. این یک coupling قوی بین دو سرویس است. در آینده با Moq و DI این مشکل حل می‌شود.

8. **ذخیره‌سازی Gender به‌صورت string:** در `Person` مدل دامنه، `Gender` از نوع `string?` است نه enum. این تصمیم قابل بحث است. در DTO‌ها از `GenderOptions?` استفاده می‌شود و در متد `ToPerson` با `.ToString()` به string تبدیل می‌شود. این یک فرصت بهبود برای خواننده است.

9. **متد `Equals` در `PersonResponse`:** برای امکان مقایسه در `Assert.Equal` override شده. نکته: فیلد `Age` در مقایسه لحاظ نمی‌شود (که منطقی است چون Age محاسبه‌شده است و ممکن است در زمان‌های مختلف متفاوت باشد).

10. **بهینه‌سازی در DeletePerson:** چون `person` پیدا شده، می‌توان مستقیماً `_persons.Remove(person)` کرد. استفاده از `RemoveAll` برای اطمینان از حذف همه‌ی موارد منطبق (در حالت تئوریک چندتایی) است.

11. **تست DeletePerson null input:** رفتار `null` بودن ورودی به‌طور جداگانه در `PersonsServiceTest` تست نشده است. این یک فرصت بهبود برای پوشش کامل‌تر است.

12. **PersonExtensions فقط `ToPersonResponse` دارد:** متد کمکی برای بازگشت از `PersonResponse` به `Person` وجود ندارد. این به این دلیل است که `PersonResponse` برای خروجی است و نباید به مدل دامنه برگردد (یک‌طرفه بودن تبدیل).

---

## ۱۵. جمع‌بندی نهایی

این سند، تجمیع کاملی از توضیحات دو پوشه از دوره‌ی Udemy-AspNetCore-Harsha است که فرآیند یادگیری و تکامل یک اپلیکیشن CRUD را در ASP.NET Core نشان می‌دهد. **پوشه‌ی ۰۸** نقطه‌ی شروع ساده‌ای بود که تمرکز آن بر پیاده‌سازی و تست متد `GetCountryByCountryID` در سرویس Countries بود. در این پوشه فقط یک موجودیت (`Country`) و یک سرویس معرفی شد و مناسب یادگیری مفاهیم پایه‌ی معماری لایه‌ای، DTO Pattern و تست‌نویسی با xUnit بود.

**پوشه‌ی ۲۶** نقطه‌ی پایان مسیر آموزشی CRUD بود. در این پوشه، علاوه بر موجودیت `Country`، موجودیت `Person` نیز اضافه شده و تمام عملیات CRUD (Add, GetAll, GetById, Filter, Sort, Update, Delete) برای آن پیاده‌سازی و تست شد. این پوشه شامل ۲ موجودیت، ۵ DTO، ۲ enum، ۲ سرویس، ۱ helper و ۲ کلاس تست است و نزدیک به یک اپلیکیشن واقعی است.

**الگوهای طراحی به‌کار رفته** شامل Layered Architecture, DTO Pattern, Interface-Based Design, Extension Methods, AAA Test Pattern, Composition, Declarative Validation با DataAnnotations, Pattern Matching در Switch Expression و Computed Fields در DTO است. **ذخیره‌سازی** فعلاً in-memory است و در آینده با EF Core جایگزین می‌شود. **تست‌ها** با xUnit و با الگوی AAA نوشته شده‌اند و رفتار سرویس را در حالت‌های مختلف (null, valid, invalid, edge case) می‌سنجند.

این ساختار، یک نقطه‌ی شروع تمیز برای توسعه‌ی واقعی است که در قسمت‌های بعدی دوره به تدریج با EF Core, Moq, Repository Pattern, Clean Architecture, Identity و JWT کامل می‌شود. در این نقطه، تمام عملیات CRUD پایه برای موجودیت Person پیاده‌سازی و تست شده است و اپلیکیشن آماده‌ی اتصال به UI واقعی و ذخیره‌ساز دائمی است. مسیر یادگیری توصیه‌شده: ابتدا با پوشه‌ی ۰۸ مفاهیم پایه را یاد بگیرید، سپس با پوشه‌ی ۲۶ با یک اپلیکیشن کامل آشنا شوید و در نهایت قسمت‌های بعدی دوره (EntityFrameworkCore, Moq, Repository Pattern) را دنبال کنید تا یک معمار واقعی ASP.NET Core شوید.
