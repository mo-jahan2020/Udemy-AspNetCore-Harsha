# پروژه‌ی آموزشی JWT — ASP.NET Core 10 + Razor Pages + EF Core + SQL Express

پیاده‌سازی کامل و ساده‌ی **JWT (JSON Web Token)** در یک برنامه‌ی Razor Pages که فقط یک صفحه‌ی اصلی (اشخاص) دارد.

## امکانات

- ثبت‌نام، ورود و خروج
- محافظت از صفحه‌ی «اشخاص» با ویژگی `[Authorize]` (پکیج `Microsoft.AspNetCore.Authorization`)
- تولید و اعتبارسنجی کامل توکن JWT (امضا با HmacSha256)
- توکن تمدید (Refresh Token) با ذخیره‌ی هش در دیتابیس، چرخش (Rotation) و تمدید خودکار
- الگوی Repository با اینترفیس جنریک + UnitOfWork + EF Core (Code First)
- فقط CSS آفلاین محلی — بدون هیچ CDN، فونت آنلاین یا جاوااسکریپت
- کامنت‌های آموزشی شماره‌گذاری‌شده (گام 1 تا 22) فقط روی بخش‌های JWT که **ترتیب پیاده‌سازی** را نشان می‌دهند

## پیش‌نیازها

1. **.NET 10 SDK**
2. **SQL Server Express** با نام ایننس پیش‌فرض `SQLEXPRESS`

## اجرا

1. اگر نام ایننس SQL Server شما متفاوت است، رشته‌ی اتصال را در `appsettings.json` اصلاح کنید:

```json
"ConnectionStrings": {
  "SqlServer": "Server=.\\SQLEXPRESS;Database=JwtDemoDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

- برای LocalDB: `"Server=(localdb)\\MSSQLLocalDB;..."`
- برای سرور SQL کامل: `"Server=localhost;..."`

2. پروژه را اجرا کنید:

```bash
dotnet run
```

3. مرورگر را باز کنید: **http://localhost:7221**

> پورت 7221 با مقدار `Issuer` در تنظیمات JWT یکسان است؛ آن را تغییر ندهید.

دیتابیس در اولین اجرا به صورت خودکار ساخته و با چند شخص نمونه پر می‌شود
(برای سادگی دمو از `EnsureCreated` استفاده شده؛ در پروژه‌ی واقعی از Migration استفاده کنید).

## سناریوی پیشنهادی برای دیدن JWT در عمل

1. **ثبت‌نام** کنید → به صفحه‌ی اشخاص هدایت می‌شوید.
2. در **پنل آموزشی JWT** (ستون کناری) توکن سه‌بخشی، هدر، Payload و Claims را ببینید.
3. شمارش معکوس انقضای توکن دسترسی (۱ دقیقه) را ببینید.
4. بعد از انقضا **صفحه را رفرش کنید** → تمدید خودکار انجام می‌شود و توکن جدید صادر می‌گردد (گام 21) — حتی کوکی‌ها هم تمدید می‌شوند.
5. دکمه‌ی **«صدور توکن جدید (Refresh)»** را بزنید → چرخش دستی توکن‌ها (گام 20).
6. بیش از ۶۰ دقیقه بی‌فعالیت بمانید → توکن تمدید هم منقضی می‌شود → هدایت به ورود.
7. **خروج** → توکن تمدید در دیتابیس باطل و کوکی‌ها حذف می‌شوند (گام 22).

## نقشه‌ی گام‌های پیاده‌سازی JWT

| گام | موضوع | توضیح | فایل |
|----|--------------------------|------------------------------------------------------------------|---------------------------|
| 1 | JWT Basics | ساختار سه‌بخشی توکن: Header.Payload.Signature | `Services/TokenService.cs` |
| 2 | JWT Basics | مفهوم Claims (ادعاهای هویتی کاربر) | `Services/TokenService.cs` |
| 3 | JWT Basics | خواندن تنظیمات JWT از appsettings.json | `Services/TokenService.cs` |
| 4 | Generating JWT Tokens | ساخت کلید امضای متقارن (SymmetricSecurityKey) | `Services/TokenService.cs` |
| 5 | Generating JWT Tokens | اعتبارنامه‌ی امضا (SigningCredentials با HmacSha256) | `Services/TokenService.cs` |
| 6 | Generating JWT Tokens | ساخت لیست Claims کاربر | `Services/TokenService.cs` |
| 7 | Generating JWT Tokens | ساخت شیء JwtSecurityToken (Issuer/Audience/انقضا) | `Services/TokenService.cs` |
| 8 | Generating JWT Tokens | تبدیل توکن به رشته‌ی نهایی با WriteToken | `Services/TokenService.cs` |
| 9 | Authorization with JWT | ثبت احراز هویت: AddAuthentication + AddJwtBearer | `Program.cs` |
| 10 | Authorization with JWT | معیارهای اعتبارسنجی: TokenValidationParameters | `Program.cs` |
| 11 | Authorization with JWT | خواندن توکن از کوکی در OnMessageReceived (برای Razor Pages) | `Program.cs` |
| 12 | Authorization with JWT | UseAuthentication و UseAuthorization در خط لوله | `Program.cs` |
| 13 | Authorization with JWT | محافظت از صفحه با ویژگی [Authorize] | `Pages/Persons.cshtml.cs` |
| 14 | Authorization with JWT | خواندن هویت کاربر از ClaimsPrincipal در صفحه | `Pages/Persons.cshtml.cs` |
| 15 | Refresh Tokens | چرا توکن تمدید؟ (توکن ۱ دقیقه‌ای + تمدید ۶۰ دقیقه‌ای) | `Services/TokenService.cs` |
| 16 | Refresh Tokens | تولید مقدار تصادفی امن با RandomNumberGenerator | `Services/TokenService.cs` |
| 17 | Refresh Tokens | ذخیره‌ی «هش» توکن تمدید در دیتابیس با تاریخ انقضا | `Services/AuthService.cs` |
| 18 | Refresh Tokens | قرار دادن توکن‌ها در کوکی‌های HttpOnly | `Services/AuthService.cs` |
| 19 | Refresh Tokens | اعتبارسنجی توکن تمدید (جستجو در دیتابیس، انقضا، ابطال) | `Services/AuthService.cs` |
| 20 | Refresh Tokens | چرخش توکن (Rotation): صدور جفت توکن جدید و باطل کردن قبلی | `Services/AuthService.cs` |
| 21 | Refresh Tokens | تمدید خودکار هنگام انقضای توکن دسترسی | `Program.cs` |
| 22 | Refresh Tokens | خروج: ابطال توکن تمدید و حذف کوکی‌ها | `Services/AuthService.cs` |

## ساختار پروژه

```
JwtDemo/
├── Program.cs                  ← گام‌های 9 تا 12 و 21
├── appsettings.json            ← تنظیمات Jwt و RefreshToken (دقیقاً مطابق خواسته)
├── Models/
│   ├── Person.cs               ← مدل دامنه (نام، نام خانوادگی، سن)
│   ├── User.cs                 ← کاربر (هش رمز عبور)
│   └── RefreshToken.cs         ← توکن تمدید (هش، انقضا، ابطال)
├── Data/
│   └── AppDbContext.cs         ← DbContext برنامه
├── Repositories/
│   ├── IRepository.cs          ← اینترفیس جنریک Repository
│   ├── Repository.cs           ← پیاده‌سازی عمومی
│   ├── IPersonRepository.cs / PersonRepository.cs
│   ├── IUserRepository.cs / UserRepository.cs
│   ├── IRefreshTokenRepository.cs / RefreshTokenRepository.cs
│   └── IUnitOfWork.cs / UnitOfWork.cs
├── Security/
│   └── PasswordHasher.cs       ← هش رمز عبور (غیرمرتبط با JWT)
├── Services/
│   ├── ITokenService.cs / TokenService.cs     ← گام‌های 1 تا 8 و 15 و 16
│   └── IAuthService.cs / AuthService.cs       ← گام‌های 17 تا 20 و 22
├── Pages/
│   ├── Shared/_Layout.cshtml   ← لایه‌ی مشترک صفحات
│   ├── Login.cshtml(.cs)       ← ورود
│   ├── Register.cshtml(.cs)    ← ثبت‌نام
│   ├── Logout.cshtml(.cs)      ← خروج (گام 22)
│   └── Persons.cshtml(.cs)     ← تنها صفحه‌ی اصلی (گام‌های 13 و 14)
└── wwwroot/css/site.css        ← CSS کاملاً آفلاین
```

## نکات امنیتی (برای استفاده‌ی واقعی)

- کلید تنظیم‌شده در `appsettings.json` کوتاه‌تر از حداقل ۲۵۶ بیتِ HMAC-SHA256 است؛ کد به صورت آموزشی آن را با تکرار به طول کافی می‌رساند (`TokenService.CreateSigningKey`). در پروژه‌ی واقعی یک کلید طولانی و تصادفی قرار دهید و آن را در تنظیمات نگه ندارید.
- هش رمز عبور با SHA256 ساده انجام شده؛ در پروژه‌ی واقعی از BCrypt یا PBKDF2 استفاده کنید.
- برنامه روی HTTP اجرا می‌شود تا دمو ساده بماند؛ در Production از HTTPS استفاده کنید و `Secure = true` را به کوکی‌ها اضافه کنید.
- چرخش توکن (Rotation) باعث می‌شود هر توکن تمدید فقط یک بار قابل استفاده باشد؛ اگر کاربر با چند تب همزمان کار کند، تب‌های دیگر ممکن است به صفحه‌ی ورود هدایت شوند (بده‌بستان امنیتی متداول).
- از `EnsureCreated` فقط برای دمو استفاده شده؛ برای پروژه‌ی واقعی Migration بسازید:
  `dotnet ef migrations add Init` سپس `dotnet ef database update`
