# پروژه‌ی آموزشی JWT — نسخه‌ی MVC + Swagger

ASP.NET Core 10 · **MVC** (کنترلر + View) · **Swagger** · EF Core · SQL Express

بازنویسی همان پروژه‌ی آموزشی JWT با مکانیزم MVC و افزودن Swagger برای تست تعاملی APIها با توکن JWT.

## امکانات

- **معماری MVC**: کنترلرها (`HomeController`, `AccountController`, `PersonController`) + Viewها با `_Layout.cshtml`
- **رابط کاربری وب**: ثبت‌نام، ورود، خروج و صفحه‌ی «اشخاص» با پنل آموزشی زنده‌ی JWT (توکن سه‌بخشی، هدر، Payload، Claims، شمارش معکوس انقضا)
- **Swagger UI** در آدرس **/swagger** با دکمه‌ی **Authorize** برای تست APIها با توکن JWT
- **REST API**: `api/auth` (ثبت‌نام، ورود، تمدید، خروج) و `api/persons` (CRUD کامل محافظت‌شده با JWT)
- **دو مسیر احراز هویت با یک مکانیزم JWT**:
  - صفحات MVC → توکن در کوکی HttpOnly (مرورگر نمی‌تواند هدر Authorization بفرستد) + تمدید خودکار
  - API و Swagger → توکن در هدر `Authorization: Bearer ...` (استاندارد JWT)
- الگوی Repository با اینترفیس جنریک + UnitOfWork + EF Core (Code First)
- محافظت با `[Authorize]` از پکیج `Microsoft.AspNetCore.Authorization`
- فقط CSS آفلاین محلی — بدون هیچ CDN، فونت آنلاین یا جاوااسکریپت
- کامنت‌های آموزشی شماره‌گذاری‌شده (گام 1 تا 22) فقط روی بخش‌های JWT

## پیش‌نیازها

1. **.NET 10 SDK**
2. **SQL Server Express** با نام ایننس پیش‌فرض `SQLEXPRESS`

## اجرا

1. اگر نام ایننس SQL Server شما متفاوت است، رشته‌ی اتصال را در `appsettings.json` اصلاح کنید:

```json
"ConnectionStrings": {
  "SqlServer": "Server=.\\SQLEXPRESS;Database=JwtDemoMvcDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

- برای LocalDB: `"Server=(localdb)\\MSSQLLocalDB;..."`

2. اجرا:

```bash
dotnet run
```

3. مرورگر: **http://localhost:7221** (رابط کاربری) و **http://localhost:7221/swagger** (Swagger)

> پورت 7221 با مقدار `Issuer` در تنظیمات JWT یکسان است؛ آن را تغییر ندهید.

دیتابیس در اولین اجرا به‌صورت خودکار ساخته و با چند شخص نمونه پر می‌شود (برای سادگی دمو از `EnsureCreated` استفاده شده؛ در پروژه‌ی واقعی از Migration استفاده کنید).

## سناریو 1: رابط کاربری MVC (توکن در کوکی)

1. **ثبت‌نام** کنید → به صفحه‌ی اشخاص هدایت می‌شوید.
2. در **پنل آموزشی JWT** توکن سه‌بخشی، هدر، Payload و Claims را ببینید.
3. شمارش معکوس انقضای توکن دسترسی (۱ دقیقه) را ببینید.
4. بعد از انقضا **صفحه را رفرش کنید** → تمدید خودکار: جفت توکن جدید با توکن تمدید صادر می‌شود (گام 21).
5. دکمه‌ی **«صدور توکن جدید (Refresh)»** → چرخش دستی توکن‌ها (گام 20).
6. بیش از ۶۰ دقیقه بی‌فعالیت → توکن تمدید هم منقضی می‌شود → هدایت به ورود.
7. **خروج** → ابطال توکن تمدید و حذف کوکی‌ها (گام 22).

## سناریو 2: تست JWT در Swagger (توکن در هدر — حالت کلاسیک API)

1. به **/swagger** بروید.
2. `POST /api/auth/register` یا `POST /api/auth/login` را اجرا کنید → پاسخ شامل `accessToken` و `refreshToken`.
3. روی دکمه‌ی **Authorize** کلیک کنید، مقدار `accessToken` را وارد کنید و Authorize بزنید.
4. `GET /api/persons` را اجرا کنید → پاسخ 200 با لیست اشخاص (هدر `Authorization: Bearer ...` خودکار ارسال می‌شود).
5. **۱ دقیقه صبر کنید** و دوباره `GET /api/persons` را بزنید → پاسخ **401** (توکن دسترسی منقضی شده).
6. `POST /api/auth/refresh` را با `refreshToken` اجرا کنید → جفت توکن جدید (چرخش — گام 20).
7. توکن جدید را در Authorize وارد کنید و ادامه دهید.
8. `POST /api/auth/logout` با `refreshToken` → ابطال توکن تمدید (گام 22)؛ از این به بعد refresh با 401 جواب می‌دهد.

## نقشه‌ی گام‌های پیاده‌سازی JWT (مثل نسخه‌ی قبل، بدون تغییر)

| گام | موضوع | توضیح | فایل |
|----|--------------------------|------------------------------------------------------------------|----------------------------------|
| 1 | JWT Basics | ساختار سه‌بخشی توکن: Header.Payload.Signature | `Services/TokenService.cs` |
| 2 | JWT Basics | مفهوم Claims (ادعاهای هویتی کاربر) | `Services/TokenService.cs` |
| 3 | JWT Basics | خواندن تنظیمات JWT از appsettings.json | `Services/TokenService.cs` |
| 4 | Generating JWT Tokens | کلید امضای متقارن (SymmetricSecurityKey) | `Services/TokenService.cs` |
| 5 | Generating JWT Tokens | اعتبارنامه‌ی امضا (SigningCredentials با HmacSha256) | `Services/TokenService.cs` |
| 6 | Generating JWT Tokens | ساخت لیست Claims کاربر | `Services/TokenService.cs` |
| 7 | Generating JWT Tokens | ساخت شیء JwtSecurityToken (Issuer/Audience/انقضا) | `Services/TokenService.cs` |
| 8 | Generating JWT Tokens | تبدیل توکن به رشته‌ی نهایی با WriteToken | `Services/TokenService.cs` |
| 9 | Authorization with JWT | ثبت احراز هویت: AddAuthentication + AddJwtBearer | `Program.cs` |
| 10 | Authorization with JWT | معیارهای اعتبارسنجی: TokenValidationParameters | `Program.cs` |
| 11 | Authorization with JWT | خواندن توکن از هدر Authorization یا کوکی (OnMessageReceived) | `Program.cs` |
| 12 | Authorization with JWT | UseAuthentication و UseAuthorization در خط لوله | `Program.cs` |
| 13 | Authorization with JWT | محافظت با [Authorize] (کنترلر + API) | `Controllers/PersonController.cs` و `Controllers/Api/PersonsController.cs` |
| 14 | Authorization with JWT | خواندن هویت کاربر از ClaimsPrincipal | `Controllers/PersonController.cs` |
| 15 | Refresh Tokens | چرا توکن تمدید؟ (توکن ۱ دقیقه‌ای + تمدید ۶۰ دقیقه‌ای) | `Services/TokenService.cs` |
| 16 | Refresh Tokens | تولید مقدار تصادفی امن با RandomNumberGenerator | `Services/TokenService.cs` |
| 17 | Refresh Tokens | ذخیره‌ی «هش» توکن تمدید در دیتابیس با تاریخ انقضا | `Services/AuthService.cs` |
| 18 | Refresh Tokens | قرار دادن توکن‌ها در کوکی‌های HttpOnly (رابط کاربری) | `Services/AuthService.cs` |
| 19 | Refresh Tokens | اعتبارسنجی توکن تمدید (جستجو در دیتابیس، انقضا، ابطال) | `Services/AuthService.cs` |
| 20 | Refresh Tokens | چرخش توکن (Rotation): صدور جفت توکن جدید و باطل کردن قبلی | `Services/AuthService.cs` |
| 21 | Refresh Tokens | تمدید خودکار هنگام انقضای توکن دسترسی | `Program.cs` |
| 22 | Refresh Tokens | خروج: ابطال توکن تمدید و حذف کوکی‌ها | `Services/AuthService.cs` |

> بخش Swagger در `Program.cs` جزو پیاده‌سازی JWT نیست و کامنت شماره‌دار ندارد؛ فقط در `api/auth` و `api/persons` به گام‌های مرتبط ارجاع داده شده است.

## ساختار پروژه

```
JwtDemoMVC/
├── Program.cs                     ← گام‌های 9 تا 12 و 21 + پیکربندی MVC و Swagger
├── appsettings.json               ← تنظیمات Jwt و RefreshToken (دقیقاً مطابق خواسته)
├── Models/
│   ├── Person.cs                  ← مدل دامنه
│   ├── User.cs                    ← کاربر (هش رمز عبور)
│   ├── RefreshToken.cs            ← توکن تمدید (هش، انقضا، ابطال)
│   └── ApiContracts.cs            ← مدل‌های درخواست API
├── Data/AppDbContext.cs
├── Repositories/                  ← الگوی Repository با اینترفیس + UnitOfWork
├── Security/PasswordHasher.cs
├── Services/
│   ├── TokenService.cs            ← گام‌های 1 تا 8 و 15 و 16
│   └── AuthService.cs             ← گام‌های 17 تا 22 (دو گروه متد: کوکی و API)
├── ViewModels/                    ← مدل‌های فرم و نمایش MVC
├── Controllers/
│   ├── HomeController.cs          ← هدایت به صفحه‌ی اشخاص
│   ├── AccountController.cs       ← ورود / ثبت‌نام / خروج
│   ├── PersonController.cs        ← گام‌های 13 و 14 (صفحه‌ی اصلی)
│   └── Api/
│       ├── AuthController.cs      ← api/auth (توکن‌ها برای Swagger)
│       └── PersonsController.cs   ← api/persons (CRUD محافظت‌شده با JWT)
├── Views/
│   ├── Shared/_Layout.cshtml
│   ├── Account/Login.cshtml · Register.cshtml
│   └── Person/Index.cshtml        ← صفحه‌ی اشخاص + پنل آموزشی JWT
└── wwwroot/css/site.css           ← CSS کاملاً آفلاین
```

## نکات فنی و امنیتی

- **Swashbuckle نسخه‌ی 10.3.1**: مبتنی بر Microsoft.OpenApi 2.x است؛ به همین دلیل الگوی `AddSecurityRequirement` با نسخه‌های قدیمی (۶ تا ۹) فرق دارد — در `Program.cs` از الگوی جدید (`OpenApiSecuritySchemeReference` + delegate) استفاده شده است.
- کلید تنظیم‌شده در `appsettings.json` کوتاه‌تر از حداقل ۲۵۶ بیتِ HMAC-SHA256 است؛ کد به‌صورت آموزشی آن را با تکرار به طول کافی می‌رساند (`TokenService.CreateSigningKey`). در پروژه‌ی واقعی کلید طولانی و تصادفی قرار دهید.
- هش رمز عبور با SHA256 ساده است؛ در پروژه‌ی واقعی از BCrypt یا PBKDF2 استفاده کنید.
- برنامه روی HTTP اجرا می‌شود تا دمو ساده بماند؛ در Production از HTTPS استفاده کنید و `Secure = true` به کوکی‌ها اضافه کنید.
- چرخش توکن (Rotation) هر توکن تمدید را فقط یک‌بار قابل استفاده می‌کند؛ تب‌های موازی ممکن است به ورود هدایت شوند.
- از `EnsureCreated` فقط برای دمو استفاده شده؛ برای پروژه‌ی واقعی: `dotnet ef migrations add Init` سپس `dotnet ef database update`
