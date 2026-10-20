# راهنمای گام‌به‌گام ساخت پروژه CRUD با xUnit

> **هدف:** این فایل به‌عنوان نقشه‌ی راه برای ساخت همین پروژه از صفر استفاده می‌شود. به‌ترتیب گام‌ها را دنبال کنید و هر بخش را خودتان بنویسید تا یاد بگیرید.
>
> **پیش‌نیاز:** آشنیت اولیه با C# و Visual Studio / VS Code.

---

## 🗺 نقشه‌ی راه کلی

```text
گام ۱: ساخت Solution و ۵ پروژه + تنظیم وابستگی‌ها
گام ۲: ساخت لایه‌ی Entities (مدل‌های دامنه)
گام ۳: ساخت لایه‌ی ServiceContracts (DTO‌ها، Enum‌ها، اینترفیس‌ها)
گام ۴: ساخت لایه‌ی Services (پیاده‌سازی سرویس‌ها)
گام ۵: ساخت لایه‌ی CRUDExample (Host وب)
گام ۶: ساخت لایه‌ی CRUDTests (تست‌های واحد)
گام ۷: اجرای تست‌ها و دیباگ
```

---

## گام ۱: ساخت Solution و پروژه‌ها

### ۱.۱ دستورات ایجاد

```bash
dotnet new sln -n CRUDSolution

# ۱. پروژه‌ی Entities (Class Library)
dotnet new classlib -n Entities -f net10.0

# ۲. پروژه‌ی ServiceContracts
dotnet new classlib -n ServiceContracts -f net10.0

# ۳. پروژه‌ی Services
dotnet new classlib -n Services -f net10.0

# ۴. پروژه‌ی CRUDExample (وب)
dotnet new web -n CRUDExample -f net10.0

# ۵. پروژه‌ی CRUDTests (xUnit)
dotnet new xunit -n CRUDTests -f net10.0
```

### ۱.۲ تنظیم وابستگی‌ها (References)

به ترتیب زیر وابستگی‌ها را اضافه کنید:

```bash
# ServiceContracts ← Entities
dotnet add ServiceContracts reference Entities

# Services ← ServiceContracts
dotnet add Services reference ServiceContracts

# CRUDTests ← هر سه
dotnet add CRUDTests reference Entities
dotnet add CRUDTests reference ServiceContracts
dotnet add CRUDTests reference Services
```

### ۱.۳ فعال‌سازی Nullable Reference Types

در تمام فایل‌های `.csproj` این سه خط را اضافه کنید:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

> **چرا این ترتیب؟** چون وابستگی‌ها یک‌طرفه‌اند (از پایین به بالا). هرگز پروژه‌ی پایین (مثل `Entities`) نباید به پروژه‌ی بالا ارجاع کند — این کار باعث Circular Reference می‌شود.

---

## گام ۲: لایه‌ی Entities (مدل‌های دامنه)

این لایه فقط شامل کلاس‌های ساده‌ی داده‌ای (POCO) است. هیچ منطقی ندارند — فقط داده‌ها را نگه می‌دارند.

### ۲.۱ `Country.cs`

**منطق کلاس:** یک کشور با شناسه‌ی یکتا و نام.

```csharp
namespace Entities
{
  public class Country
  {
    public Guid CountryID { get; set; }
    public string? CountryName { get; set; }
  }
}
```

### ۲.۲ `Person.cs`

**منطق کلاس:** یک شخص با اطلاعات تماس و ارتباط به کشور.

```csharp
namespace Entities
{
  public class Person
  {
    public Guid PersonID { get; set; }
    public string? PersonName { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }       // توجه: string ذخیره می‌شود، نه enum
    public Guid? CountryID { get; set; }        // کلید خارجی به Country
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }
  }
}
```

> **نکته‌ی آموزشی:** چرا `Gender` از نوع string است نه enum؟ چون مدل دامنه باید مستقل از لایه‌های دیگر باشد. enum در ServiceContracts تعریف می‌شود (در گام بعد). این جداسازی نشان‌دهنده‌ی اصل DDD (Domain-Driven Design) است.

---

## گام ۳: لایه‌ی ServiceContracts

این لایه شامل سه بخش است: **DTO‌ها**، **Enum‌ها** و **اینترفیس‌ها**.

### ۳.۱ Enum‌ها (اول بنویسید — چون DTO‌ها به آن‌ها نیاز دارند)

#### `Enums/GenderOptions.cs`

```csharp
namespace ServiceContracts.Enums
{
  public enum GenderOptions
  {
    Male, Female, Other
  }
}
```

#### `Enums/SortOrderOptions.cs`

```csharp
namespace ServiceContracts.Enums
{
  public enum SortOrderOptions
  {
    ASC, DESC
  }
}
```

### ۳.۲ DTO‌ها

#### `DTO/CountryAddRequest.cs` — ورودی برای Add

```csharp
using Entities;

namespace ServiceContracts.DTO
{
  public class CountryAddRequest
  {
    public string? CountryName { get; set; }

    // متد تبدیل: DTO → مدل دامنه
    public Country ToCountry()
    {
      return new Country() { CountryName = CountryName };
    }
  }
}
```

#### `DTO/CountryResponse.cs` — خروجی

```csharp
using Entities;

namespace ServiceContracts.DTO
{
  public class CountryResponse
  {
    public Guid CountryID { get; set; }
    public string? CountryName { get; set; }

    // برای امکان مقایسه در Assert.Equal
    public override bool Equals(object? obj) { /* پیاده‌سازی کن */ }
    public override int GetHashCode() => base.GetHashCode();
  }

  // Extension Method برای تبدیل مدل دامنه → DTO
  public static class CountryExtensions
  {
    public static CountryResponse ToCountryResponse(this Country country)
    {
      return new CountryResponse()
      {
        CountryID = country.CountryID,
        CountryName = country.CountryName
      };
    }
  }
}
```

#### `DTO/PersonAddRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO
{
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
      return new Person()
      {
        PersonName = PersonName,
        Email = Email,
        DateOfBirth = DateOfBirth,
        Gender = Gender.ToString(),   // enum → string
        Address = Address,
        CountryID = CountryID,
        ReceiveNewsLetters = ReceiveNewsLetters
      };
    }
  }
}
```

#### `DTO/PersonUpdateRequest.cs`

تفاوت با AddRequest: شامل `PersonID` است.

```csharp
public class PersonUpdateRequest
{
  [Required] public Guid PersonID { get; set; }
  [Required] public string? PersonName { get; set; }
  [Required][EmailAddress] public string? Email { get; set; }
  public DateTime? DateOfBirth { get; set; }
  public GenderOptions? Gender { get; set; }
  public Guid? CountryID { get; set; }
  public string? Address { get; set; }
  public bool ReceiveNewsLetters { get; set; }

  public Person ToPerson() { /* مشابه PersonAddRequest ولی با PersonID */ }
}
```

#### `DTO/PersonResponse.cs`

```csharp
using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO
{
  public class PersonResponse
  {
    public Guid PersonID { get; set; }
    public string? PersonName { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public Guid? CountryID { get; set; }
    public string? Country { get; set; }       // نام کشور (lookup می‌شود)
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }
    public double? Age { get; set; }            // محاسبه‌شده از DateOfBirth

    public override bool Equals(object? obj) { /* مقایسه فیلدها */ }
    public override string ToString() { /* فرمت خروجی */ }

    // برای فرم Edit: Response → UpdateRequest
    public PersonUpdateRequest ToPersonUpdateRequest() { /* پیاده‌سازی */ }
  }

  public static class PersonExtensions
  {
    public static PersonResponse ToPersonResponse(this Person person)
    {
      return new PersonResponse()
      {
        PersonID = person.PersonID,
        PersonName = person.PersonName,
        Email = person.Email,
        DateOfBirth = person.DateOfBirth,
        Gender = person.Gender,
        CountryID = person.CountryID,
        Address = person.Address,
        ReceiveNewsLetters = person.ReceiveNewsLetters,
        // محاسبه سن: تقسیم بر 365.25 برای لحاظ سال کبیسه
        Age = (person.DateOfBirth != null)
          ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25)
          : null
      };
    }
  }
}
```

> **نکته‌ی آموزشی:** چرا DTO جدا از مدل دامنه داریم؟ چون لایه‌ی بیرونی (UI/API) نباید جزئیات داخلی دامنه را بداند. این کار Encapsulation و امنیت را بالا می‌برد.

### ۳.۳ اینترفیس‌ها

#### `ICountriesService.cs`

```csharp
using ServiceContracts.DTO;

namespace ServiceContracts
{
  public interface ICountriesService
  {
    CountryResponse AddCountry(CountryAddRequest? countryAddRequest);
    List<CountryResponse> GetAllCountries();
    CountryResponse? GetCountryByCountryID(Guid? countryID);
  }
}
```

#### `IPersonsService.cs`

```csharp
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
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
    bool DeletePerson(Guid? personID);
  }
}
```

> **چرا اینترفیس؟** تا قرارداد و پیاده‌سازی جدای شوند. این کار امکان Mock کردن در تست را فراهم می‌کند.

---

## گام ۴: لایه‌ی Services (پیاده‌سازی)

### ۴.۱ اول `Helpers/ValidationHelper.cs` را بنویسید

**منطق:** اجرای اعتبارسنجی declarative با DataAnnotations.

```csharp
using System.ComponentModel.DataAnnotations;

namespace Services.Helpers
{
  public class ValidationHelper
  {
    // internal یعنی فقط از داخل همین پروژه قابل دسترسی است
    internal static void ModelValidation(object obj)
    {
      ValidationContext ctx = new ValidationContext(obj);
      List<ValidationResult> results = new List<ValidationResult>();

      bool isValid = Validator.TryValidateObject(obj, ctx, results, true);
      if (!isValid)
        throw new ArgumentException(results.FirstOrDefault()?.ErrorMessage);
    }
  }
}
```

### ۴.۲ سپس `CountriesService.cs`

**منطق:** سرویس مدیریت کشورها با ذخیره‌سازی in-memory.

```csharp
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
  public class CountriesService : ICountriesService
  {
    private readonly List<Country> _countries;

    public CountriesService()
    {
      _countries = new List<Country>();
    }

    public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
    {
      // ۱. Validation: null نباشد
      if (countryAddRequest == null)
        throw new ArgumentNullException(nameof(countryAddRequest));

      // ۲. Validation: CountryName null نباشد
      if (countryAddRequest.CountryName == null)
        throw new ArgumentException(nameof(countryAddRequest.CountryName));

      // ۳. Validation: تکراری نباشد
      if (_countries.Any(c => c.CountryName == countryAddRequest.CountryName))
        throw new ArgumentException("Given country name already exists");

      // ۴. تبدیل DTO به مدل دامنه
      Country country = countryAddRequest.ToCountry();

      // ۵. تولید ID
      country.CountryID = Guid.NewGuid();

      // ۶. افزودن به لیست
      _countries.Add(country);

      // ۷. برگرداندن به‌صورت Response
      return country.ToCountryResponse();
    }

    public List<CountryResponse> GetAllCountries()
    {
      return _countries.Select(c => c.ToCountryResponse()).ToList();
    }

    public CountryResponse? GetCountryByCountryID(Guid? countryID)
    {
      if (countryID == null) return null;

      Country? country = _countries.FirstOrDefault(c => c.CountryID == countryID);
      if (country == null) return null;

      return country.ToCountryResponse();
    }
  }
}
```

### ۴.۳ سپس `PersonsService.cs`

**منطق:** سرویس مدیریت اشخاص. به `CountriesService` وابسته است (Composition) چون نام کشور را از آن می‌گیرد.

```csharp
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services
{
  public class PersonsService : IPersonsService
  {
    private readonly List<Person> _persons;
    private readonly ICountriesService _countriesService;

    public PersonsService()
    {
      _persons = new List<Person>();
      _countriesService = new CountriesService(); // Manual injection (در آینده با DI)
    }

    // متد کمکی برای تبدیل + پر کردن نام کشور
    private PersonResponse ConvertPersonToPersonResponse(Person person)
    {
      PersonResponse response = person.ToPersonResponse();
      response.Country =
        _countriesService.GetCountryByCountryID(person.CountryID)?.CountryName;
      return response;
    }

    public PersonResponse AddPerson(PersonAddRequest? req)
    {
      if (req == null)
        throw new ArgumentNullException(nameof(req));

      // اعتبارسنجی با DataAnnotations
      ValidationHelper.ModelValidation(req);

      Person person = req.ToPerson();
      person.PersonID = Guid.NewGuid();
      _persons.Add(person);

      return ConvertPersonToPersonResponse(person);
    }

    public List<PersonResponse> GetAllPersons()
    {
      return _persons.Select(p => p.ToPersonResponse()).ToList();
    }

    public PersonResponse? GetPersonByPersonID(Guid? personID)
    {
      if (personID == null) return null;

      Person? person = _persons.FirstOrDefault(p => p.PersonID == personID);
      return person?.ToPersonResponse();
    }

    public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
    {
      List<PersonResponse> all = GetAllPersons();

      if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
        return all;

      // با switch روی searchBy، فیلتر مناسب را اجرا کنید
      // مثلاً برای PersonName: all.Where(p => p.PersonName?.Contains(searchString, ...) == true)
      // ... (برای همه‌ی فیلدها)
      return all;
    }

    public List<PersonResponse> GetSortedPersons(
      List<PersonResponse> allPersons,
      string sortBy,
      SortOrderOptions sortOrder)
    {
      if (string.IsNullOrEmpty(sortBy)) return allPersons;

      // Switch Expression با Tuple Pattern
      return (sortBy, sortOrder) switch
      {
        (nameof(PersonResponse.PersonName), SortOrderOptions.ASC)
          => allPersons.OrderBy(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
        (nameof(PersonResponse.PersonName), SortOrderOptions.DESC)
          => allPersons.OrderByDescending(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
        // ... برای Email, DateOfBirth, Age, Gender, Country, Address, ReceiveNewsLetters
        _ => allPersons
      };
    }

    public PersonResponse UpdatePerson(PersonUpdateRequest? req)
    {
      if (req == null)
        throw new ArgumentNullException(nameof(req));

      ValidationHelper.ModelValidation(req);

      Person? matching = _persons.FirstOrDefault(p => p.PersonID == req.PersonID);
      if (matching == null)
        throw new ArgumentException("Given person id doesn't exist");

      // به‌روزرسانی فیلدها
      matching.PersonName = req.PersonName;
      matching.Email = req.Email;
      matching.DateOfBirth = req.DateOfBirth;
      matching.Gender = req.Gender.ToString();
      matching.CountryID = req.CountryID;
      matching.Address = req.Address;
      matching.ReceiveNewsLetters = req.ReceiveNewsLetters;

      return matching.ToPersonResponse();
    }

    public bool DeletePerson(Guid? personID)
    {
      if (personID == null)
        throw new ArgumentNullException(nameof(personID));

      Person? person = _persons.FirstOrDefault(p => p.PersonID == personID);
      if (person == null) return false;

      _persons.RemoveAll(p => p.PersonID == personID);
      return true;
    }
  }
}
```

---

## گام ۵: لایه‌ی CRUDExample (Host)

فعلاً فقط پایه‌ی برنامه را می‌سازیم. رجیستر کردن سرویس‌ها در قسمت‌های بعدی دوره انجام می‌شود.

### `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
  app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.MapControllers();
app.Run();
```

---

## گام ۶: لایه‌ی CRUDTests (تست‌ها)

### ۶.۱ الگوی کلی هر تست

هر تست از الگوی **AAA** پیروی می‌کند:

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
  // Arrange — آماده‌سازی داده‌ها

  // Act — فراخوانی متد

  // Assert — بررسی نتیجه
}
```

### ۶.۲ `CountriesServiceTest.cs`

**منطق:** در سازنده، سرویس را نمونه‌سازی می‌کنیم تا در همه‌ی تست‌ها قابل استفاده باشد.

```csharp
public class CountriesServiceTest
{
  private readonly ICountriesService _countriesService;

  public CountriesServiceTest()
  {
    _countriesService = new CountriesService();
  }

  // متدهای تست:
  // AddCountry_NullCountry → باید ArgumentNullException پرتاب کند
  // AddCountry_CountryNameIsNull → باید ArgumentException
  // AddCountry_DuplicateCountryName → باید ArgumentException
  // AddCountry_ProperCountryDetails → باید موفق شود و CountryID غیرخالی برگرداند
  // GetAllCountries_EmptyList → باید لیست خالی برگرداند
  // GetAllCountries_AddFewCountries → باید همه‌ی موارد اضافه‌شده را برگرداند
  // GetCountryByCountryID_NullCountryID → باید null برگرداند
  // GetCountryByCountryID_ValidCountryID → باید همان کشور برگردانده شود
}
```

**نمونه‌ی تست:**

```csharp
[Fact]
public void AddCountry_NullCountry()
{
  //Arrange
  CountryAddRequest? request = null;

  //Assert
  Assert.Throws<ArgumentNullException>(() =>
  {
    //Act
    _countriesService.AddCountry(request);
  });
}
```

### ۶.۳ `PersonsServiceTest.cs`

**منطق:** نیاز به دو سرویس دارد (`PersonsService` + `CountriesService`) چون برای افزودن Person به CountryID معتبر نیاز داریم. همچنین از `ITestOutputHelper` برای چاپ خروجی دیباگ استفاده می‌کند.

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

  // تست‌های محوری (۱۵ تست):
  // AddPerson: NullPerson, PersonNameIsNull, ProperPersonDetails
  // GetPersonByPersonID: NullPersonID, WithPersonID
  // GetAllPersons: EmptyList, AddFewPersons
  // GetFilteredPersons: EmptySearchText, SearchByPersonName
  // GetSortedPersons (یک تست)
  // UpdatePerson: NullPerson, InvalidPersonID, PersonNameIsNull, FullDetailsUpdation
  // DeletePerson: ValidPersonID, InvalidPersonID
}
```

**نمونه‌ی تست DeletePerson:**

```csharp
[Fact]
public void DeletePerson_ValidPersonID()
{
  // Arrange
  CountryResponse country = _countriesService.AddCountry(
    new CountryAddRequest() { CountryName = "USA" });

  PersonResponse person = _personService.AddPerson(new PersonAddRequest()
  {
    PersonName = "Jones",
    Email = "jones@example.com",
    Address = "address",
    CountryID = country.CountryID,
    DateOfBirth = Convert.ToDateTime("2010-01-01"),
    Gender = GenderOptions.Male,
    ReceiveNewsLetters = true
  });

  // Act
  bool isDeleted = _personService.DeletePerson(person.PersonID);

  // Assert
  Assert.True(isDeleted);
}

[Fact]
public void DeletePerson_InvalidPersonID()
{
  bool isDeleted = _personService.DeletePerson(Guid.NewGuid());
  Assert.False(isDeleted);
}
```

---

## گام ۷: اجرا و تست

### ۷.۱ اجرای تست‌ها

```bash
dotnet test
```

### ۷.۲ اجرای وب اپلیکیشن

```bash
cd CRUDExample
dotnet run
```

سپس به `http://localhost:5298` بروید (پورت از `launchSettings.json` می‌آید).

---

## 📋 جدول متدها و ورودی/خروجی‌ها

| متد                       | ورودی                                  | خروجی                    | خطاهای ممکن                                       |
|---------------------------|----------------------------------------|--------------------------|---------------------------------------------------|
| `AddCountry`              | `CountryAddRequest?`                   | `CountryResponse`       | `ArgumentNullException`, `ArgumentException`       |
| `GetAllCountries`         | —                                      | `List<CountryResponse>` | —                                                 |
| `GetCountryByCountryID`   | `Guid? countryID`                      | `CountryResponse?`      | — (null برمی‌گردد اگر ورودی یا کشور پیدا نشود)   |
| `AddPerson`               | `PersonAddRequest?`                    | `PersonResponse`        | `ArgumentNullException`, `ArgumentException` (validation) |
| `GetAllPersons`           | —                                      | `List<PersonResponse>`  | —                                                 |
| `GetPersonByPersonID`     | `Guid? personID`                       | `PersonResponse?`       | —                                                 |
| `GetFilteredPersons`      | `string searchBy`, `string? searchString` | `List<PersonResponse>` | —                                                 |
| `GetSortedPersons`        | `List<PersonResponse>`, `string sortBy`, `SortOrderOptions sortOrder` | `List<PersonResponse>` | —                                                 |
| `UpdatePerson`            | `PersonUpdateRequest?`                 | `PersonResponse`        | `ArgumentNullException`, `ArgumentException`       |
| `DeletePerson`            | `Guid? personID`                       | `bool`                  | `ArgumentNullException` (اگر ورودی null باشد)      |

---

## 🎯 نکات طلایی برای یادگیری

### ۱. ترتیب یادگیری
1. ابتدا مدل دامنه (`Entities`) را بنویسید — چون پایه‌ی همه چیز است
2. سپس DTO‌ها را بنویسید — چون اینترفیس‌ها به آن‌ها نیاز دارند
3. سپس اینترفیس‌ها را تعریف کنید — چون سرویس‌ها به آن‌ها نیاز دارند
4. سپس پیاده‌سازی سرویس‌ها را انجام دهید
5. در نهایت تست‌ها را بنویسید

### ۲. الگوی AAA در تست
همیشه تست‌ها را به سه بخش `Arrange` / `Act` / `Assert` تقسیم کنید — حتی با کامنت. این کار خوانایی را بالا می‌برد.

### ۳. قانون طلایی وابستگی
- `Entities` هیچ وابستگی ندارد
- `ServiceContracts` فقط به `Entities` وابسته است
- `Services` به `ServiceContracts` وابسته است (و غیرمستقیم به `Entities`)
- هرگز وابستگی را معکوس نکنید!

### ۴. تبدیل DTO ↔ Domain
- هرگز مدل دامنه را مستقیم به بیرون return نکنید — همیشه از DTO استفاده کنید
- برای تبدیل از Extension Methods (`ToCountryResponse()`, `ToPersonResponse()`) استفاده کنید
- تبدیل DTO → Domain با متد `ToCountry()` / `ToPerson()` انجام می‌شود

### ۵. اعتبارسنجی
- در DTO‌ها از `[Required]`, `[EmailAddress]` و سایر DataAnnotations استفاده کنید
- با `ValidationHelper.ModelValidation` آن‌ها را اجرا کنید
- این کار با یک خط کد، تمام validationهای همه‌ی فیلدها را اجرا می‌کند

### ۶. چرا `Equals` را override کنیم؟
- برای اینکه `Assert.Equal` در تست بتواند دو DTO را با مقایسه‌ی فیلدها (نه رفرنسی) مقایسه کند
- بدون این کار، تست شکست می‌خورد

### ۷. Composition در سرویس‌ها
- `PersonsService` به `ICountriesService` وابسته است چون نام کشور را از آن می‌گیرد
- فعلاً به‌صورت `new CountriesService()` در سازنده ایجاد می‌شود — در آینده با DI جایگزین می‌شود

### ۸. Switch Expression برای Sort
به‌جای if-else طولانی، از این الگو استفاده کنید:

```csharp
return (sortBy, sortOrder) switch
{
  ("PersonName", SortOrderOptions.ASC) => allPersons.OrderBy(...).ToList(),
  ("PersonName", SortOrderOptions.DESC) => allPersons.OrderByDescending(...).ToList(),
  _ => allPersons  // default
};
```

### ۹. منابع برای یادگیری عمیق‌تر
- **LINQ** — `FirstOrDefault`, `Where`, `OrderBy`, `Select`, `Any`
- **Extension Methods** در C#
- **Pattern Matching** و Switch Expression
- **DataAnnotations** برای اعتبارسنجی
- **xUnit** و `Assert.Equal`, `Assert.Throws`, `Assert.True`, `Assert.Null`

### ۱۰. تمرین‌های پیشنهادی
پس از ساخت پروژه، این تمرین‌ها را انجام دهید:

1. **بهبود `GetHashCode`:** وقتی `Equals` را override می‌کنید، `GetHashCode` هم باید منطق متناظر داشته باشد.
2. **استفاده از `Any` به‌جای `Where().Count() > 0`:** کارآمدتر است.
3. **افزودن تست برای DeletePerson با null input:** فعلاً تست نشده است.
4. **افزودن validation در `UpdatePerson` برای ID نامعتبر:** قبل از فراخوانی، بررسی کنید آیا شخص وجود دارد.
5. **تبدیل `Gender` در مدل دامنه به enum:** تصمیم بهتر طراحی است.

---

## 📝 خلاصه‌ی سریع معماری

```text
┌─────────────────────────────────┐
│      CRUDExample (Host)          │  ← Programs
└─────────────────────────────────┘
              ▲
              │ uses (در آینده)
              ▼
┌─────────────────────────────────┐
│       CRUDTests (Tests)          │  ← Tests
└─────────────────────────────────┘
              ▲
              │ tests
              ▼
┌─────────────────────────────────┐
│         Services                 │  ← Business Logic
│  CountriesService, PersonsService │
└─────────────────────────────────┘
              ▲
              │ implements
              ▼
┌─────────────────────────────────┐
│      ServiceContracts            │  ← Interfaces + DTOs + Enums
└─────────────────────────────────┘
              ▲
              │ references
              ▼
┌─────────────────────────────────┐
│          Entities                │  ← Domain Models
│      Country, Person             │
└─────────────────────────────────┘
```

پیشنهاد می‌کنم پروژه را به‌ترتیب بالا بسازید و در هر گام، کد را خودتان بنویسید (کپی نکنید!). وقتی به بن‌بست رسیدید، به این راهنما برگردید و بخش مربوطه را مجدداً بخوانید. این روش یادگیری شما را عمیق می‌کند.
