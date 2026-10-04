# 🎓 پروژه آموزشی تزریق وابستگی (DI) در ASP.NET Core 10

این پروژه یک نمونه آموزشی کامل برای یاد تزریق وابستگی در ASP.NET Core MVC است
که با کامنت‌های فارسی و مثال‌های عملی، مفاهیم زیر را نشان می‌دهد:

## ✨ امکانات

- **نسخه دات‌نت:** 10.0
- **نوع پروژه:** Standard MVC
- **زبان کامنت‌ها:** فارسی

## 📚 مفاهیم آموزشی

### ۱) Lifetimeهای مختلف سرویس‌ها

| Lifetime | توضیح | مناسب برای |
|----------|-------|------------|
| **Transient** | هر بار درخواست → یک نمونه جدید | سرویس‌های سبک، Stateless |
| **Scoped** | در یک HTTP Request ثابت، بین Requestها متفاوت | DbContext، UnitOfWork |
| **Singleton** | در کل عمر برنامه فقط یک نمونه | Cache، Configuration |

### ۲) انواع تزریق وابستگی

- **Constructor Injection** (رایج‌ترین): وابستگی‌ها به‌صورت پارامتر Constructor
- **Method Injection**: وابستگی به‌صورت پارامتر متد (با `[FromServices]` در اکشن متدها)
- **Property Injection**: با استفاده از Factory در زمان ثبت سرویس

### ۳) سرویس‌های نمونه

- سرویس‌های سفارشی با اینترفیس: `IOperationGuid`, `IOrderProcessor`
- سرویس‌های داخلی ASP.NET Core: `ILogger<T>`, `IWebHostEnvironment`

## 📁 ساختار پروژه

```
DIEduDemo/
├── Controllers/
│   └── HomeController.cs              # کنترلر اصلی با انواع تزریق
├── Models/
│   └── LifetimeComparisonModel.cs     # ViewModel برای صفحه مقایسه
├── Services/
│   ├── IOperationGuid.cs              # اینترفیس مشترک + سه اینترفیس مشتق‌شده
│   ├── OperationGuidService.cs        # پیاده‌سازی مشترک سه Lifetime
│   ├── ConstructorInjectionDemo.cs    # نمونه Constructor Injection
│   ├── MethodInjectionDemo.cs         # نمونه Method Injection
│   ├── PropertyInjectionDemo.cs       # نمونه Property Injection
│   ├── LifetimeInspector.cs           # سرویس مقایسه Lifetime
│   ├── IOrderProcessor.cs             # اینترفیس سرویس بیزینسی
│   └── OrderProcessor.cs              # پیاده‌سازی سرویس بیزینسی
├── Views/
│   ├── Home/Index.cshtml              # فرم خروجی مقایسه
│   ├── Shared/_Layout.cshtml          # قالب کلی
│   └── Shared/Error.cshtml            # صفحه خطا
├── wwwroot/css/site.css               # استایل صفحه
├── Program.cs                         # ثبت سرویس‌ها و پیکربندی
├── DIEduDemo.csproj                   # فایل پروژه
└── appsettings.json                   # تنظیمات
```

## 🚀 نحوه اجرا

### پیش‌نیازها
- نصب [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### اجرای پروژه

```bash
cd DIEduDemo
dotnet run
```

سپس مرورگر را روی آدرس زیر باز کنید:

```
http://localhost:5000
```

یا با `dotnet run --urls "http://localhost:5000"` پورت دلخواه تعیین کنید.

## 🔍 چه چیزی در خروجی ببینید؟

### صفحه اصلی (`/`)
- جدول مقایسه هر سه Lifetime با Guid واقعی
- نمایش نتیجه Constructor/Method/Property Injection
- فراخوانی سرویس بیزینسی `IOrderProcessor`

### صفحه MethodInjectionExample (`/Home/MethodInjectionExample`)
- نمایش استفاده از `[FromServices]` برای تزریق در پارامتر اکشن متد

## 🧪 نکات آموزشی

### چرا سه اینترفیس مجزا برای Lifetimeها؟
اگر هر سه سرویس فقط `IOperationGuid` را پیاده‌سازی کنند، Container نمی‌داند کدام را تزریق کند.
راه حل: سه اینترفیس مشتق‌شده تعریف می‌کنیم:
- `IOperationTransient : IOperationGuid`
- `IOperationScoped : IOperationGuid`
- `IOperationSingleton : IOperationGuid`

### چرا Singletonها باید Thread-safe باشند؟
چون در کل عمر برنامه فقط یک نمونه ساخته می‌شود و چندین Request همزمان به آن دسترسی دارند.
اگر State داخلی تغییر کند، باید از `lock` یا مکانیزم‌های همگام‌سازی استفاده شود.

### چرا Property Injection توصیه نمی‌شود؟
- وابستگی‌ها اختیاری می‌شوند و ممکن است null باشند.
- Container پیش‌فرض ASP.NET Core آن را پشتیبانی نمی‌کند.
- تست‌پذیری کاهش می‌یابد.

## 📝 تمرین‌های پیشنهادی

1. یک سرویس Scoped جدید بسازید که State را در طول Request نگه دارد.
2. یک سرویس Singleton با `lock` برای شمارش Requestها پیاده‌سازی کنید.
3. با `AddKeyedSingleton` (در دات‌نت 8+) یک Keyed Service بسازید.
4. با `Scrutor` یا `Autofac` Property Injection واقعی را امتحان کنید.

## 🎯 خلاصه

این پروژه نشان داد که چطور با ثبت سرویس‌ها در `Program.cs` و تزریق آن‌ها در Constructor/Method/Property،
می‌توان کدی Loose Coupling، Testable و انعطاف‌پذیر داشت.
