# توضیح ساختار پروژه و ارتباط سرویس‌ها و کلاس‌ها

> **مسیر پوشه‌ی بررسی‌شده در ریپازیتوری:**
> `14. xUnit / 26. Delete Person - Implementation`
>
> **منبع:** `https://github.com/mo-jahan2020/Udemy-AspNetCore-Harsha`
>
> **محور این قسمت:** پیاده‌سازی و تست متد `DeletePerson` در `PersonsService` به‌علاوه‌ی پشتیبانی کامل CRUD برای موجودیت Person.

---

## ۱. نمای کلی Solution

این پوشه حاوی فایل Solution به نام `CRUDSolution.sln` است که مانند قسمت‌های قبلی شامل **۵ پروژه‌ی مجزای .NET 10** می‌باشد، اما با تفاوت کلیدی: پروژه‌ها بسیار بزرگ‌تر شده‌اند و منطق کامل CRUD برای موجودیت **Person** را در خود جای داده‌اند.

| # | نام پروژه         | نوع پروژه                       | نقش (Responsibility)                                   |
|---|-------------------|----------------------------------|--------------------------------------------------------|
| 1 | `CRUDExample`     | Web (ASP.NET Core MVC – `Microsoft.NET.Sdk.Web`) | میزبان (Host) وب برای اجرای اپلیکیشن                   |
| 2 | `CRUDTests`       | Test Project (xUnit)             | نوشته شدن تست‌های واحد برای سرویس‌های Countries و Persons |
| 3 | `Entities`        | Class Library                    | مدل دامنه (Domain Model) — حالا شامل `Country` و `Person` |
| 4 | `ServiceContracts`| Class Library                    | اینترفیس‌های سرویس، DTO‌ها و Enum‌ها                  |
| 5 | `Services`        | Class Library                    | پیاده‌سازی سرویس‌های `CountriesService` و `PersonsService` به همراه `ValidationHelper` |

### ۱.۱ مشخصات فنی پروژه‌ها

تمام پروژه‌ها از تنظیمات مشابه زیر برخوردارند:

```xml
<TargetFramework>net10.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```

- **Framework:** .NET 10
- **Implicit Usings:** فعال
- **Nullable Reference Types:** فعال

پروژه‌ی `CRUDTests` به این پکیج‌های NuGet وابسته است:
- `Microsoft.NET.Test.Sdk` نسخه 18.0.1
- `xunit` نسخه 2.9.3
- `xunit.runner.visualstudio` نسخه 3.1.5
- `coverlet.collector` نسخه 6.0.4 (سنجش پوشش کد)

---

## ۲. ساختار درختی پوشه‌ها و فایل‌ها

```text
26. Delete Person - Implementation/
│
├── CRUDSolution.sln                              # فایل Solution (اشاره به ۵ پروژه)
│
├── Entities/
│   ├── Entities.csproj                           # ارجاع پروژه ندارد (پایه‌ای‌ترین لایه)
│   ├── Country.cs                                # مدل دامنه Country
│   └── Person.cs                                 # مدل دامنه Person (جدید در این قسمت)
│
├── ServiceContracts/
│   ├── ServiceContracts.csproj                   # ارجاع: → Entities
│   ├── ICountriesService.cs                      # اینترفیس سرویس Countries
│   ├── IPersonsService.cs                        # اینترفیس سرویس Persons (جدید)
│   ├── DTO/
│   │   ├── CountryAddRequest.cs                  # DTO ورودی برای Add Country
│   │   ├── CountryResponse.cs                    # DTO خروجی Country + Extension
│   │   ├── PersonAddRequest.cs                   # DTO ورودی برای Add Person
│   │   ├── PersonUpdateRequest.cs                # DTO ورودی برای Update Person
│   │   └── PersonResponse.cs                     # DTO خروجی Person + Extension
│   └── Enums/
│       ├── GenderOptions.cs                      # Male, Female, Other
│       └── SortOrderOptions.cs                   # ASC, DESC
│
├── Services/
│   ├── Services.csproj                           # ارجاع: → ServiceContracts
│   ├── CountriesService.cs                       # پیاده‌سازی ICountriesService
│   ├── PersonsService.cs                         # پیاده‌سازی IPersonsService (محور این قسمت)
│   └── Helpers/
│       └── ValidationHelper.cs                   # کمکی اعتبارسنجی مدل با DataAnnotations
│
├── CRUDExample/
│   ├── CRUDExample.csproj                        # وب‌پروژه (SDK = Microsoft.NET.Sdk.Web)
│   ├── Program.cs                                # نقطه‌ی ورود (Minimal Hosting)
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Properties/
│   │   └── launchSettings.json                   # تنظیمات اجرا روی localhost:5298
│   └── wwwroot/
│       └── StyleSheet.css                        # فایل استاتیک
│
└── CRUDTests/
    ├── CRUDTests.csproj                          # ارجاع: → Entities, ServiceContracts, Services
    ├── CountriesServiceTest.cs                   # تست‌های CountriesService
    ├── PersonsServiceTest.cs                     # تست‌های PersonsService (محور این قسمت)
    ├── UnitTest1.cs                              # تست نمونه‌ی قدیمی (MyMath)
    └── MyMath.cs                                 # کلاس کمکی برای تست ابتدایی
```

### ۲.۱ فهرست کلاس‌ها به تفکیک پروژه

| پروژه             | کلاس‌ها و فایل‌ها                                                                              |
|-------------------|-------------------------------------------------------------------------------------------------|
| `Entities`         | `Country`, `Person`                                                                            |
| `ServiceContracts` | `ICountriesService`, `IPersonsService`, `CountryAddRequest`, `CountryResponse`, `PersonAddRequest`, `PersonUpdateRequest`, `PersonResponse`, `GenderOptions`, `SortOrderOptions`, `CountryExtensions`, `PersonExtensions` |
| `Services`         | `CountriesService`, `PersonsService`, `ValidationHelper`                                       |
| `CRUDExample`      | `Program` (Top-level statements)                                                               |
| `CRUDTests`        | `CountriesServiceTest`, `PersonsServiceTest`, `UnitTest1`, `MyMath`                           |

---

## ۳. وابستگی‌های بین پروژه‌ها (Project References)

گراف وابستگی پروژه‌ها همان الگوی لایه‌ای آشناست، اما حالا ابعاد بسیار بزرگ‌تری دارد:

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
   │      │      └─── CRUDExample (تنها پروژه‌ی وب، فعلاً سرویسی رجیستر نکرده)
```

### ۳.۱ جدول وابستگی‌ها

| پروژه             | ارجاع مستقیم به                              |
|-------------------|-----------------------------------------------|
| `Entities`         | — (هیچ)                                       |
| `ServiceContracts` | `Entities`                                    |
| `Services`         | `ServiceContracts` (و غیرمستقیم `Entities`)  |
| `CRUDTests`        | `Entities`, `ServiceContracts`, `Services`    |
| `CRUDExample`      | — (فعلاً در این قسمت DI برای سرویس‌ها فعال نیست) |

> **نکته‌ی مهم طراحی:** زنجیره‌ی وابستگی همچنان لایه‌ای است. حتی با رشد پروژه این اصل حفظ شده است که هر لایه فقط به لایه‌ی پایین‌تر ارجاع می‌دهد. این کار از Circular Reference جلوگیری می‌کند و Maintainability را بالا می‌برد.

---

## ۴. شرح تفصیلی هر پروژه و کلاس‌های آن

### ۴.۱ پروژه‌ی `Entities` — مدل دامنه (Domain Layer)

این پروژه پایه‌ای‌ترین لایه است. در این قسمت علاوه بر `Country`، موجودیت `Person` هم اضافه شده است.

#### ۴.۱.۱ `Country.cs`

```csharp
public class Country
{
  public Guid CountryID { get; set; }
  public string? CountryName { get; set; }
}
```

#### ۴.۱.۲ `Person.cs` (جدید در این قسمت)

```csharp
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
```

**نکات طراحی:**
- `PersonID` از نوع `Guid` است (شناسه‌ی یکتای سراسری).
- `CountryID` یک کلید خارجی به موجودیت `Country` است (اما به‌صورت soft reference، نه یک navigation property واقعی؛ چون هنوز از EF Core استفاده نمی‌شود).
- `Gender` به‌صورت `string?` ذخیره می‌شود نه enum. این یک تصمیم قابل بحث است (در پروژه‌های واقعی بهتر است از enum یا جدول lookup استفاده شود) ولی برای اهداف آموزشی این کفایت می‌کند.
- `ReceiveNewsLetters` یک `bool` non-nullable است که مقدار پیش‌فرض `false` دارد.
- تمام فیلدهای رشته‌ای `nullable` هستند تا با قابلیت Nullable Reference Types سازگار باشند.

---

### ۴.۲ پروژه‌ی `ServiceContracts` — قراردادها، DTO‌ها و Enum‌ها

این پروژه به‌طور قابل‌توجه‌ای نسبت به قسمت قبل رشد کرده و شامل ۲ اینترفیس، ۵ کلاس DTO، ۲ enum و ۲ کلاس extension است.

#### ۴.۲.۱ پوشه‌ی `Enums/`

**`GenderOptions.cs`**
```csharp
public enum GenderOptions
{
  Male, Female, Other
}
```

**`SortOrderOptions.cs`**
```csharp
public enum SortOrderOptions
{
  ASC, DESC
}
```

این enum‌ها برای نوع‌داری (typing) قوی در DTO‌ها و پارامترهای متدها استفاده می‌شوند. مثلاً `PersonAddRequest.Gender` از نوع `GenderOptions?` است، و پارامتر `sortOrder` در `GetSortedPersons` از نوع `SortOrderOptions` است.

#### ۴.۲.۲ پوشه‌ی `DTO/` — کلاس‌های انتقال داده

##### `CountryAddRequest.cs` و `CountryResponse.cs`
همانند قسمت قبل (DTO ورودی و خروجی برای Country). `CountryResponse` متد `Equals` override شده دارد و کلاس `CountryExtensions` متد توسعه‌ی `ToCountryResponse` را تعریف می‌کند.

##### `PersonAddRequest.cs` (جدید)

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

##### `PersonUpdateRequest.cs` (جدید)

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

تفاوت با `PersonAddRequest`: فیلد `PersonID` اضافه شده تا مشخص کند کدام Person باید به‌روزرسانی شود.

##### `PersonResponse.cs` (جدید — DTO خروجی)

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

#### ۴.۲.۳ `ICountriesService.cs` (مانند قسمت قبل)

سه متد: `AddCountry`, `GetAllCountries`, `GetCountryByCountryID`.

#### ۴.۲.۴ `IPersonsService.cs` (جدید — محور این قسمت)

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
  bool DeletePerson(Guid? personID);                  // ← محور این قسمت
}
```

این اینترفیس قرارداد کامل CRUD برای Person را تعریف می‌کند:

| متد                  | ورودی                                  | خروجی                    | توضیح                                       |
|----------------------|----------------------------------------|--------------------------|---------------------------------------------|
| `AddPerson`          | `PersonAddRequest?`                    | `PersonResponse`         | افزودن شخص جدید                             |
| `GetAllPersons`      | —                                      | `List<PersonResponse>`   | برگرداندن همه‌ی اشخاص                       |
| `GetPersonByPersonID`| `Guid? personID`                       | `PersonResponse?`        | پیدا کردن شخص با ID مشخص                    |
| `GetFilteredPersons` | `string searchBy`, `string? searchString` | `List<PersonResponse>` | فیلتر کردن اشخاص                            |
| `GetSortedPersons`   | `List<PersonResponse>`, `string sortBy`, `SortOrderOptions sortOrder` | `List<PersonResponse>` | مرتب‌سازی اشخاص |
| `UpdatePerson`       | `PersonUpdateRequest?`                 | `PersonResponse`         | به‌روزرسانی اشخاص                           |
| `DeletePerson`       | `Guid? personID`                       | `bool`                   | حذف شخص — **محور این قسمت**                 |

> **نکته‌ی کلیدی این قسمت از دوره:** متد `DeletePerson` تازه به اینترفیس اضافه شده و در `PersonsService` پیاده‌سازی و در `PersonsServiceTest` تست شده است. سایر متدها از قسمت‌های قبلی به ارث رسیده‌اند.

---

### ۴.۳ پروژه‌ی `Services` — پیاده‌سازی سرویس‌ها

#### ۴.۳.۱ `CountriesService.cs` (مانند قسمت قبل)

پیاده‌سازی in-memory از `ICountriesService` با `List<Country>` و همان سه متد.

#### ۴.۳.۲ `Helpers/ValidationHelper.cs` (جدید)

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

#### ۴.۳.۳ `PersonsService.cs` (جدید — محور این قسمت)

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

  // ... متدهای AddPerson, GetAllPersons, GetPersonByPersonID,
  //     GetFilteredPersons, GetSortedPersons, UpdatePerson, DeletePerson
}
```

**نکات طراحی:**

1. **دو وابستگی:** این سرویس به دو لیست/سرویس وابسته است:
   - `_persons`: لیست in-memory از اشخاص
   - `_countriesService`: نمونه‌ای از `CountriesService` برای lookup نام کشور
2. **تزریق دستی (Manual Injection):** فعلاً `CountriesService` در سازنده به‌صورت دستی `new` می‌شود. این کار در آینده با Dependency Injection و Moq جایگزین می‌شود.
3. **متد کمکی `ConvertPersonToPersonResponse`:** این متد private، ابتدا `Person` را با `ToPersonResponse()` به DTO تبدیل می‌کند، سپس نام کشور را با فراخوانی `GetCountryByCountryID` از سرویس کشور پر می‌کند. این الگو Composition است (سرویس Person از سرویس Country استفاده می‌کند).

#### متدهای پیاده‌سازی‌شده:

**`AddPerson`:**
- اگر `personAddRequest` برابر `null` باشد → `ArgumentNullException`
- اعتبارسنجی مدل با `ValidationHelper.ModelValidation` (بررسی `[Required]` و `[EmailAddress]`)
- تبدیل DTO به `Person` با `ToPerson()`
- تولید `PersonID` با `Guid.NewGuid()`
- افزودن به لیست `_persons`
- بازگرداندن `PersonResponse` با پر کردن نام کشور از طریق `ConvertPersonToPersonResponse`

**`GetAllPersons`:** تبدیل همه‌ی اشخاص به `PersonResponse` و برگرداندن لیست.

**`GetPersonByPersonID`:**
- اگر `personID` برابر `null` باشد → `null`
- جستجو با LINQ `FirstOrDefault`
- اگر پیدا نشد → `null`
- در غیر این صورت → تبدیل به `PersonResponse`

**`GetFilteredPersons`:**
- اگر `searchBy` یا `searchString` خالی باشد → همه‌ی اشخاص برگردانده می‌شوند
- در غیر این صورت با `switch` روی `searchBy`:
  - `PersonName` → جستجوی `Contains` (case-insensitive)
  - `Email` → جستجوی `Contains`
  - `DateOfBirth` → تبدیل به رشته‌ی "dd MMMM yyyy" و سپس جستجو
  - `Gender` → جستجوی `Contains`
  - `CountryID` → جستجو در `Country` (نام کشور)
  - `Address` → جستجوی `Contains`
  - default → همه‌ی اشخاص

**`GetSortedPersons`:**
- اگر `sortBy` خالی باشد → همان لیست ورودی
- در غیر این صورت با Pattern Matching و Switch Expression:
  - ۱۶ حالت (۲ حالت ASC/DESC برای ۸ فیلد): `PersonName`, `Email`, `DateOfBirth`, `Age`, `Gender`, `Country`, `Address`, `ReceiveNewsLetters`
  - default → همان لیست
- استفاده از `StringComparer.OrdinalIgnoreCase` برای مرتب‌سازی رشته‌ای حساس به حروف کوچک و بزرگ

**`UpdatePerson`:**
- اگر `personUpdateRequest` برابر `null` باشد → `ArgumentNullException`
- اعتبارسنجی مدل با `ValidationHelper.ModelValidation`
- جستجوی شخص با `PersonID`
- اگر پیدا نشد → `ArgumentException("Given person id doesn't exist")`
- به‌روزرسانی همه‌ی فیلدها روی شخص پیدا‌شده
- بازگرداندن `PersonResponse`

**`DeletePerson` (محور این قسمت):**

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
1. اگر `personID` برابر `null` باشد → `ArgumentNullException` پرتاب می‌شود. این رفتار با متدهای دیگر (`GetPersonByPersonID`) متفاوت است که در آن null می‌دادیم و `null` برمی‌گرفت. دلیل تفاوت: حذف یک عملیات مخرب (Destructive) است و بهتر است صراحتاً به caller خطا بدهد تا به‌طور خاموش `false` برگرداند.
2. ابتدا با `FirstOrDefault` بررسی می‌کند که آیا شخص با این ID وجود دارد یا نه.
3. اگر وجود نداشت → `false` برمی‌گرداند.
4. اگر وجود داشت → با `RemoveAll` آن را از لیست حذف می‌کند و `true` برمی‌گرداند.
5. **نکته‌ی بهینه‌سازی:** چون `person` پیدا شده، می‌توان مستقیماً `_persons.Remove(person)` کرد. استفاده از `RemoveAll` برای اطمینان از حذف همه‌ی موارد منطبق (در حالت تئوریک چندتایی) است.

---

### ۴.۴ پروژه‌ی `CRUDExample` — وب اپلیکیشن (Host / Web Layer)

این پروژه میزبان (Host) اجرایی است.

#### `Program.cs` (Minimal Hosting Model — بدون تغییر نسبت به قسمت قبل)

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

> **نکته‌ی مهم:** در این مرحله از دوره، هنوز `ICountriesService` و `IPersonsService` در DI Container رجیستر نشده‌اند. این کار در قسمت‌های بعدی انجام می‌شود. فعلاً پروژه‌ی وب فقط یک skeleton است.

#### سایر فایل‌ها:
- `appsettings.json` و `appsettings.Development.json`: تنظیمات Logging
- `Properties/launchSettings.json`: اپلیکیشن روی `http://localhost:5298` اجرا می‌شود
- `wwwroot/StyleSheet.css`: فایل CSS استاتیک

---

### ۴.۵ پروژه‌ی `CRUDTests` — تست‌های واحد (Test Layer)

این پروژه شامل تمام تست‌های واحد است که رفتار سرویس‌های `Countries` و `Persons` را می‌سنجد.

#### ۴.۵.۱ `CountriesServiceTest.cs`

این کلاس تست همانند قسمت قبل است و شامل سه `#region` است:
- `AddCountry` (۴ تست: null, CountryName null, duplicate, proper)
- `GetAllCountries` (۲ تست: empty list, add few)
- `GetCountryByCountryID` (۲ تست: null ID, valid ID)

#### ۴.۵.۲ `PersonsServiceTest.cs` (محور این قسمت)

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

**ز) `DeletePerson` (۲ تست — محور این قسمت):**

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

> **نکته‌ی طراحی تست:** هر دو تست روی یک سناریوی خالص تمرکز دارند — یکی حالت موفق و دیگری حالت ناموفق. این رویکرد «测试‌های اتمام‌پذیری» (Happy Path + Sad Path) رفتار متد را به‌طور کامل پوشش می‌دهد. توجه کنید که رفتار `null` بودن ورودی به‌طور جداگانه توسط منطق `ArgumentNullException` در خود متد پوشش داده شده، اما در تست‌ها صریحاً تست نشده است (این یک فرصت بهبود برای خواننده است).

#### سایر فایل‌های تست:

- **`UnitTest1.cs` و `MyMath.cs`:** این دو فایل یک تست نمونه‌ی ابتدایی هستند که در ابتدای دوره نوشته شده‌اند. کلاس `MyMath` یک متد `Add` ساده دارد. این فایل‌ها ارتباط مستقیمی با منطق Person ندارند.

---

## ۵. نمودار ارتباط سرویس‌ها و کلاس‌ها

در ادامه، ارتباط کامل کلاس‌ها و سرویس‌ها را در قالب یک نمودار می‌بینید:

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│                              CRUDTests (Tests)                              │
│                                                                              │
│   PersonsServiceTest                                                         │
│   ├── depends on → IPersonsService (Contract)                              │
│   ├── depends on → ICountriesService (Contract)                            │
│   ├── instantiation → new PersonsService(), new CountriesService()         │
│   ├── uses DTO: PersonAddRequest, PersonUpdateRequest, PersonResponse     │
│   ├── uses DTO: CountryAddRequest, CountryResponse                          │
│   ├── uses Enum: GenderOptions, SortOrderOptions                          │
│   └── uses ITestOutputHelper (xUnit) for logging                            │
│                                                                              │
│   CountriesServiceTest                                                      │
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
│   │ ICountriesService   │       │ IPersonsService      │                    │
│   │ - AddCountry()      │       │ - AddPerson()        │                    │
│   │ - GetAllCountries() │       │ - GetAllPersons()    │                    │
│   │ - GetCountryBy      │       │ - GetPersonByPersonID│                    │
│   │   CountryID()       │       │ - GetFilteredPersons │                    │
│   └─────────┬───────────┘       │ - GetSortedPersons   │                    │
│             │ implements         │ - UpdatePerson       │                    │
│             │                    │ - DeletePerson       │ ← محور این قسمت  │
│             │                    └──────────┬───────────┘                    │
│             │                               │ implements                    │
│             │                               │                               │
│   ┌─────────┴───────────────────────────────┴────────────┐                 │
│   │                    DTO Folder                          │                 │
│   │  ├── CountryAddRequest  ├── CountryResponse           │                 │
│   │  │   └── ToCountry()     │   ├── Equals()             │                 │
│   │  │                       │   └── CountryExtensions    │                 │
│   │  ├── PersonAddRequest    │       └── ToCountryResponse│                 │
│   │  │   ├── [Required] attrs │                            │                 │
│   │  │   └── ToPerson()       │                            │                 │
│   │  ├── PersonUpdateRequest  │                            │                 │
│   │  │   ├── [Required] attrs │                            │                 │
│   │  │   └── ToPerson()       │                            │                 │
│   │  └── PersonResponse       │                            │                 │
│   │      ├── Equals()         │                            │                 │
│   │      ├── ToString()       │                            │                 │
│   │      ├── ToPersonUpdateRequest()                       │                 │
│   │      └── PersonExtensions.ToPersonResponse()          │                 │
│   └──────────────────────────────────────────────────────┘                  │
│                                                                              │
│   ┌──────────────────────────────────────────────────────┐                  │
│   │              Enums Folder                            │                  │
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
│   PersonsService : IPersonsService                                          │
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
│   CountriesService : ICountriesService                                      │
│   └── (مانند قسمت قبل)                                                       │
│                                                                              │
│   Helpers/ValidationHelper                                                  │
│   └── internal static ModelValidation(object)                               │
│       └── uses Validator.TryValidateObject (DataAnnotations)              │
└──────────────────────────────────────────────────────────────────────────────┘
                                  │
                                  │ uses
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                          Entities (Domain)                                  │
│                                                                              │
│   Country                                                                    │
│   ├── Guid  CountryID                                                        │
│   └── string? CountryName                                                   │
│                                                                              │
│   Person (جدید)                                                              │
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
│   Program.cs (Minimal Hosting)                                               │
│   ├── AddControllersWithViews()  (MVC registered)                           │
│   ├── UseStaticFiles()                                                       │
│   ├── MapControllers()                                                       │
│   └── (Registration of services will be added in subsequent sections)       │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## ۶. جریان اجرای متد `DeletePerson`

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

## ۷. مفاهیم و الگوهای طراحی به‌کار رفته

این پروژه، نمونه‌ی کلاسیک و تمیزی از چند الگوی طراحی و معماری است:

### ۷.۱ معماری لایه‌ای (Layered Architecture)

پروژه به چهار لایه‌ی مجزا تقسیم شده:
1. **لایه‌ی دامنه (Domain Layer):** `Entities`
2. **لایه‌ی قراردادها (Contract Layer):** `ServiceContracts` (شامل DTO‌ها و Enum‌ها)
3. **لایه‌ی پیاده‌سازی (Implementation Layer):** `Services` (شامل Helpers)
4. **لایه‌ی میزبان (Host Layer):** `CRUDExample`
5. **لایه‌ی تست (Test Layer):** `CRUDTests`

مزایا: جداسازی واضح مسئولیت‌ها (Separation of Concerns)، قابلیت تست‌پذیری بالا، قابل تعویض بودن پیاده‌سازی، جلوگیری از Circular Dependency.

### ۷.۲ الگوی DTO (Data Transfer Object)

پروژه برای هر موجودیت دو DTO دارد:
- **`PersonAddRequest`** برای ورودی Add
- **`PersonUpdateRequest`** برای ورودی Update
- **`PersonResponse`** برای خروجی همه‌ی متدها

مزایا: پنهان کردن جزئیات داخلی مدل دامنه، امکان تغییر مدل دامنه بدون شکستن API، کنترل دقیق روی فیلدهای قابل ارسال، جداسازی اعتبارسنجی ورودی از منطق دامنه.

### ۷.۳ الگوی Interface-Based Design (Program to Interface)

سرویس‌ها با اینترفیس (`IPersonsService`, `ICountriesService`) تعریف می‌شوند و پیاده‌سازی (`PersonsService`, `CountriesService`) جداگانه. تست‌ها هم به اینترفیس ارجاع می‌دهند نه پیاده‌سازی مستقیم.

### ۷.۴ Extension Methods

استفاده از `PersonExtensions.ToPersonResponse` و `CountryExtensions.ToCountryResponse` برای تبدیل، یک الگوی تمیز برای:
- نگه‌داشتن منطق تبدیل نزدیک به نوع مقصد
- کدخوانایی بالاتر (مثل `person.ToPersonResponse()`)
- قابلیت استفاده‌ی مجدد

### ۷.۵ الگوی AAA در تست‌ها

تمام تست‌های xUnit از الگوی Arrange-Act-Assert پیروی می‌کنند.

### ۷.۶ In-Memory Storage (موقت)

پیاده‌سازی فعلی از `List<Person>` و `List<Country>` به‌عنوان ذخیره‌ساز استفاده می‌کند. در آینده با EF Core جایگزین می‌شود.

### ۷.۷ Composition بین سرویس‌ها (جدید)

`PersonsService` به `ICountriesService` وابسته است تا نام کشور را برای `PersonResponse` پر کند. این یک الگوی Composition است: سرویس‌ها با هم همکاری می‌کنند. اما فعلاً این Composition به‌صورت دستی (`new CountriesService()`) انجام می‌شود که در آینده با DI جایگزین می‌شود.

### ۷.۸ Declarative Validation با DataAnnotations (جدید)

استفاده از ویژگی‌های `[Required]` و `[EmailAddress]` در DTO‌ها به‌صورت declarative و سپس اجرای آن‌ها با `ValidationHelper.ModelValidation` یک الگوی رایج در ASP.NET Core است. این کار:
- منطق اعتبارسنجی را به‌صورت Attribute روی فیلدها می‌نویسد
- با یک متد عمومی در `ValidationHelper` اجرا می‌شود
- قابل استفاده مجدد برای همه‌ی DTO‌هاست

### ۷.۹ Pattern Matching در C# (جدید)

استفاده از Switch Expression با Pattern Matching در `GetSortedPersons`:
```csharp
List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
{
  (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) => ...,
  (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) => ...,
  // ...
};
```
این الگو کد را فشرده، خوانا و قابل نگه‌داری می‌کند.

### ۷.۱۰ Computed Fields در DTO (جدید)

فیلد `Age` در `PersonResponse` به‌صورت خودکار از روی `DateOfBirth` محاسبه می‌شود (در `ToPersonResponse`). این کار:
- یک فیلد مفهومی (نه ذخیره‌سازی‌شده) را به UI عرضه می‌کند
- نیازی به ذخیره‌سازی جداگانه ندارد
- هر بار به‌روز می‌شود

---

## ۸. نکات قابل توجه در این مرحله از دوره

1. **رشد قابل‌توجه پروژه:** نسبت به قسمت ۰۸ (Get Country by Country ID)، این پوشه کلاس‌های بسیار بیشتری دارد: ۲ موجودیت، ۵ DTO، ۲ enum، ۲ سرویس، ۱ helper و ۲ کلاس تست. این نشان می‌دهد پروژه در حال نزدیک شدن به یک اپلیکیشن واقعی است.

2. **هنوز DI فعال نیست:** در `Program.cs` پروژه‌ی `CRUDExample`، سرویس‌ها رجیستر نشده‌اند. این کار در قسمت بعدی انجام می‌شود.

3. **تزریق دستی در PersonsService:** در سازنده‌ی `PersonsService`، `new CountriesService()` فراخوانی می‌شود. این یک coupling قوی بین دو سرویس است. در آینده با Moq و DI این مشکل حل می‌شود.

4. **ذخیره‌سازی Gender به‌صورت string:** در `Person` مدل دامنه، `Gender` از نوع `string?` است نه enum. این تصمیم قابل بحث است. در DTO‌ها از `GenderOptions?` استفاده می‌شود و در متد `ToPerson` با `.ToString()` به string تبدیل می‌شود. این یک فرصت بهبود برای خواننده است.

5. **متد `Equals` در `PersonResponse`:** برای امکان مقایسه در `Assert.Equal` override شده. نکته: فیلد `Age` در مقایسه لحاظ نمی‌شود (که منطقی است چون Age محاسبه‌شده است و ممکن است در زمان‌های مختلف متفاوت باشد).

6. **`GetHashCode` به‌سادگی override شده:** به `base.GetHashCode()` ارجاع می‌دهد. این کار اصولی نیست (وقتی `Equals` override می‌شود، `GetHashCode` هم باید منطق متناظر داشته باشد). این یک فرصت بهبود است.

7. **بهینه‌سازی احتمالی در DeletePerson:** چون `person` پیدا شده، می‌توان مستقیماً `_persons.Remove(person)` کرد. استفاده از `RemoveAll` برای اطمینان از حذف همه‌ی موارد منطبق (در حالت تئوریک چندتایی) است.

8. **بهینه‌سازی در AddCountry:** چک تکراری بودن نام کشور با `Where(...).Count() > 0` نوشته شده. استفاده از `Any(...)` کارآمدتر است.

9. **تست DeletePerson null input:** رفتار `null` بودن ورودی به‌طور جداگانه در `PersonsServiceTest` تست نشده است. این یک فرصت بهبود برای پوشش کامل‌تر است.

10. **PersonExtensions فقط `ToPersonResponse` دارد:** متد کمکی برای بازگشت از `PersonResponse` به `Person` وجود ندارد. این به این دلیل است که `PersonResponse` برای خروجی است و نباید به مدل دامنه برگردد (یک‌طرفه بودن تبدیل).

---

## ۹. مقایسه با قسمت قبل (۰۸. Get Country by Country ID)

| جنبه                         | قسمت ۰۸                                       | قسمت ۲۶ (این پوشه)                                            |
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
| **تعداد کل تست‌های Person**  | —                                              | ۱۴ (AddPerson ۳ + Get ۲ + GetAll ۲ + Filter ۲ + Sort ۱ + Update ۴ + Delete ۲) |
| **Composition بین سرویس‌ها** | ندارد                                          | PersonsService → CountriesService                 |
| **Pattern Matching**         | ندارد                                          | در GetSortedPersons                               |
| **ITestOutputHelper**        | استفاده نشده                                   | در PersonsServiceTest                             |

این مقایسه نشان می‌دهد که در طی ۱۸ قسمت (۰۸ تا ۲۶)، پروژه از یک سرویس ساده با ۱ موجودیت به یک اپلیکیشن CRUD کامل با ۲ موجودیت، ۷ عملیات (Add, GetAll, GetById, Filter, Sort, Update, Delete) و ۱۴ تست واحد رشد کرده است.

---

## ۱۰. جمع‌بندی

این پوشه، یک قدم اساسی در مسیر تکاملی ساخت یک اپلیکیشن CRUD با ASP.NET Core است. در این مرحله:

- **ساختار Solution** شامل ۵ پروژه‌ی مجزا با وابستگی‌های لایه‌ای است
- **معماری** از نوع Layered با جداسازی مدل دامنه، قرارداد، پیاده‌سازی، میزبان و تست است
- **محور این قسمت** پیاده‌سازی و تست متد `DeletePerson` است که به حذف یک شخص با شناسه‌ی Guid می‌پردازد
- **الگوهای طراحی** شامل DTO Pattern, Interface-Based Design, Extension Methods, AAA Test Pattern, Composition, Declarative Validation و Pattern Matching است
- **ذخیره‌سازی** فعلاً in-memory است و در آینده با EF Core جایگزین می‌شود
- **تست‌ها** با xUnit و با الگوی AAA نوشته شده‌اند و رفتار سرویس را در حالت‌های مختلف (null, valid, invalid, edge case) می‌سنجند

این ساختار، یک نقطه‌ی شروع تمیز برای توسعه‌ی واقعی است که در قسمت‌های بعدی دوره به تدریج با EF Core, Moq, Repository Pattern, Clean Architecture, Identity و JWT کامل می‌شود. در این نقطه، تمام عملیات CRUD پایه برای موجودیت Person پیاده‌سازی و تست شده است و اپلیکیشن آماده‌ی اتصال به UI واقعی و ذخیره‌ساز دائمی است.
