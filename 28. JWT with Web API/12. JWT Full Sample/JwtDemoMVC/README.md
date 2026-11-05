# پروژه‌ی آموزشی JWT — نسخه‌ی MVC + Swagger

ASP.NET Core 10 ·  MVC  (کنترلر + View) ·  Swagger  · EF Core · SQL Express

بازنویسی همان پروژه‌ی آموزشی JWT با مکانیزم MVC و افزودن Swagger برای تست تعاملی APIها با توکن JWT.

## امکانات

-  معماری MVC : کنترلرها (`HomeController`, `AccountController`, `PersonController`) + Viewها با `_Layout.cshtml`
-  رابط کاربری وب : ثبت‌نام، ورود، خروج و صفحه‌ی «اشخاص» با پنل آموزشی زنده‌ی JWT (توکن سه‌بخشی، هدر، Payload، Claims، شمارش معکوس انقضا)
-  Swagger UI  در آدرس  /swagger  با دکمه‌ی  Authorize  برای تست APIها با توکن JWT
-  REST API : `api/auth` (ثبت‌نام، ورود، تمدید، خروج) و `api/persons` (CRUD کامل محافظت‌شده با JWT)
-  دو مسیر احراز هویت با یک مکانیزم JWT :
  - صفحات MVC → توکن در کوکی HttpOnly (مرورگر نمی‌تواند هدر Authorization بفرستد) + تمدید خودکار
  - API و Swagger → توکن در هدر `Authorization: Bearer ...` (استاندارد JWT)
- الگوی Repository با اینترفیس جنریک + UnitOfWork + EF Core (Code First)
-  IP AllowList (جدید) : صفحه‌ی تنظیمات `/Settings` برای مدیریت IPهای مجاز همراه با تاریخ اتمام اعتبار، ذخیره در دیتابیس و اعمال خودکار با یک Middleware قبل از احراز هویت
- محافظت با `[Authorize]` از پکیج `Microsoft.AspNetCore.Authorization`
- فقط CSS آفلاین محلی — بدون هیچ CDN، فونت آنلاین یا جاوااسکریپت
- کامنت‌های آموزشی شماره‌گذاری‌شده (گام 1 تا 22) فقط روی بخش‌های JWT

## پیش‌نیازها

1.  .NET 10 SDK 
2.  SQL Server Express  با نام ایننس پیش‌فرض `SQLEXPRESS`

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

3. مرورگر:  http://localhost:7221  (رابط کاربری) و  http://localhost:7221/swagger  (Swagger)

> پورت 7221 با مقدار `Issuer` در تنظیمات JWT یکسان است؛ آن را تغییر ندهید.

دیتابیس در اولین اجرا به‌صورت خودکار ساخته و با چند شخص نمونه پر می‌شود (برای سادگی دمو از `EnsureCreated` استفاده شده؛ در پروژه‌ی واقعی از Migration استفاده کنید).

## سناریو 1: رابط کاربری MVC (توکن در کوکی)

1.  ثبت نام  کنید → به صفحه‌ی اشخاص هدایت می‌شوید.
2. در  پنل آموزشی JWT  توکن سه‌بخشی، هدر، Payload و Claims را ببینید.
3. شمارش معکوس انقضای توکن دسترسی (۱ دقیقه) را ببینید.
4. بعد از انقضا  صفحه را رفرش کنید  → تمدید خودکار: جفت توکن جدید با توکن تمدید صادر می‌شود (گام 21).
5. دکمه‌ی  «صدور توکن جدید (Refresh)»  → چرخش دستی توکن‌ها (گام 20).
6. بیش از ۶۰ دقیقه بی‌فعالیت → توکن تمدید هم منقضی می‌شود → هدایت به ورود.
7.  خروج  → ابطال توکن تمدید و حذف کوکی‌ها (گام 22).

## سناریو 2: تست JWT در Swagger (توکن در هدر — حالت کلاسیک API)

1. به  /swagger  بروید.
2. `POST /api/auth/register` یا `POST /api/auth/login` را اجرا کنید → پاسخ شامل `accessToken` و `refreshToken`.
3. روی دکمه‌ی  Authorize  کلیک کنید، مقدار `accessToken` را وارد کنید و Authorize بزنید.
4. `GET /api/persons` را اجرا کنید → پاسخ 200 با لیست اشخاص (هدر `Authorization: Bearer ...` خودکار ارسال می‌شود).
5.  ۱ دقیقه صبر کنید  و دوباره `GET /api/persons` را بزنید → پاسخ  401  (توکن دسترسی منقضی شده).
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
│   ├── AllowedIp.cs               ← IP مجاز (تاریخ انقضا) — ویژگی IP AllowList
│   └── ApiContracts.cs            ← مدل‌های درخواست API
├── Data/AppDbContext.cs
├── Repositories/                  ← الگوی Repository با اینترفیس + UnitOfWork
├── Middleware/
│   └── IpAllowListMiddleware.cs   ← ویژگی IP AllowList (قبل از احراز هویت)
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
│       ├── PersonsController.cs   ← api/persons (CRUD محافظت‌شده با JWT)
│       └── DebugController.cs     ← api/debug/token-check (ابزار آموزشی اعتبارسنجی — فقط Development)
├── Views/
│   ├── Shared/_Layout.cshtml
│   ├── Account/Login.cshtml · Register.cshtml
│   ├── Settings/Index.cshtml      ← تنظیمات IPهای مجاز + تاریخ انقضا
│   ├── Settings/Denied.cshtml     ← صفحه‌ی «دسترسی رد شد»
│   └── Person/Index.cshtml        ← صفحه‌ی اشخاص + پنل آموزشی JWT
└── wwwroot/css/site.css           ← CSS کاملاً آفلاین (در مخزن اصلی جا افتاده بود — بازسازی شد)
```

## چرا تغییر Issuer توکن‌ها را باطل نمی‌کند و Audience مانع استفاده‌ی بقیه نمی‌شود؟ (دو اشتباه رایج)

### اشتباه ۱ — «ValidateIssuer باید جلوی ساختنِ توکن را بگیرد»
`ValidateIssuer` فقط «توکنِ رسیده» را بررسی می‌کند، نه فرآیند تولید را. ساختنِ رشته‌ی JWT هیچ رازی لازم ندارد (دو بخش اول توکن فقط Base64 هستند)؛ هر کسی می‌تواند توکنی با هر iss/aud دلخواه بسازد. چیزی که جعل را بی‌فایده می‌کند «امضای دیجیتال» است (`ValidateIssuerSigningKey`): توکنِ امضاشده با کلید اشتباه — حتی با iss/aud کاملاً درست — با 401 رد می‌شود.

پس چرا با تغییر Issuer به 7222 توکن‌های Swagger همچنان معتبر ماندند؟ چون `TokenService` (تولید) و `Program.cs` (اعتبارسنجی)  هر دو از همان یک مقدار appsettings می‌خوانند ؛ تغییر عدد، هر دو سمت را با هم عوض می‌کند. برای دیدن رد شدنِ واقعی: سرور را روی 7222 نگه دارید و توکنی با iss=7221 (با همان کلید) بفرستید → 401 با `IDX10214: Issuer validation failed`.

### اشتباه ۲ — «ValidAudience یعنی فقط کلاینتِ localhost:4200 مجاز است»
`aud` یک ادعای *داخل توکن* است، نه آدرس فراخوان؛ سرور هیچ‌وقت آدرس/Origin فرستنده را با aud مقایسه نمی‌کند. JWT از نوع  Bearer  است: هر کس توکن را «داشته باشد» تا انقضا می‌تواند استفاده کند — چه از localhost:4200 چه 7221. کاربرد واقعی aud جلوگیری از استفاده‌ی توکنِ یک API در APIی دیگر است (توکنِ aud=A در سروری که ValidAudience=B دارد رد می‌شود). برای محدودکردن مرورگرها CORS و برای محدودکردن کلاینت‌ها IP Allowlist/mTLS به‌کار می‌رود، نه aud.

### چهار آزمایش یک‌دقیقه‌ای (بدون تغییر کد)
کلید این دمو دقیقاً ۳۲ کاراکتر است، پس می‌توانید در [jwt.io](https://jwt.io) با همان کلیدِ `appsettings.json` توکن دستی بسازید و از دکمه‌ی Authorize به سرور بفرستید:

| # | تغییر در jwt.io | نتیجه در سرور | خطا |
|---|------------------|----------------|------|
| 1 | iss → `http://localhost:9999` | 401 | IDX10214 (Issuer) |
| 2 | aud → `http://localhost:9999` | 401 | IDX10215 (Audience) |
| 3 | یک کاراکتر از secret کلید | 401 | IDX10603 (Signature) — دیوار واقعی جلوی جعل |
| 4 | exp در گذشته | 401 | IDX10223 (Expired) |

- دلیل دقیق هر 401 در  لاگ کنسول  برنامه هم چاپ می‌شود (کد IDX).
- برای دیدن گزارش ساخت‌یافته‌ی «کدام بررسی شکست خورد»: `POST /api/debug/token-check` با بدنه‌ی `{ "token": "..." }` (فقط در Development فعال است).

### نکته‌ی امنیتی مهم‌تر از هر دو مورد
چون کلید امضا داخل `appsettings.json` و داخل یک مخزن  عمومی  است، هر کسی که این مخزن را دیده باشد می‌تواند توکنِ کاملاً معتبر (با iss/aud درست) بسازد — این محدودیت دموی آموزشی است، نه نقص ValidateIssuer. در پروژه‌ی واقعی: کلید ۶۴+ کاراکتری تصادفی در user-secrets/متغیر محیطی/Key Vault، و کلید را هرگز Commit نکنید.

## ویژگی IP AllowList — IPهای مجاز همراه با تاریخ انقضا (Middleware + صفحه تنظیمات)

از این نسخه، «استفاده از JWT» فقط از IPهای مجازِ ثبت‌شده در دیتابیس ممکن است:

-  صفحه تنظیمات `/Settings`  (داخل منوی بالای صفحه هم اضافه شده): جدول IPهای مجاز با ستون‌های آدرس، توضیح،  تاریخ اتمام اعتبار  (تقویم شمسی + زمان باقی‌مانده) و وضعیت فعال/منقضی؛ فرم افزودن IP جدید با «مدت اعتبار (دقیقه)»؛ دکمه‌ی حذف؛ و بنر «IP فعلی شما» با دکمه‌ی افزودن سریع.
-  ذخیره در دیتابیس : جدول `AllowedIps` (با ایندکس یونیک روی IP) + Repository مستقل و اتصال به UnitOfWork.
-  اعمال با Middleware  (`Middleware/IpAllowListMiddleware.cs`) که قبل از احراز هویت در خط لوله قرار دارد:

| | |
|---|---|
| مسیرهای محافظت‌شده | `/Person` · `/Settings` · `/api/persons` · `/api/debug` |
| مسیرهای مستثنی | ورود/ثبت‌نام · `api/auth` · Swagger · فایل‌های استاتیک · `/Settings/Denied` |
| رفتار رد شدن در API | پاسخ JSON با کد 403 + نمایش IP فراخوان |
| رفتار رد شدن در MVC | ریدایرکت به صفحه‌ی «دسترسی رد شد» با نمایش IP کاربر |
| کارایی | لیست با کش ۳۰ ثانیه‌ای (`IMemoryCache`) خوانده می‌شود؛ با هر Add/Delete کش بی‌درنگ باطل می‌شود |

### چرا Middleware؟ (جواب سوال طراحی)
محدودسازی IP یک نگرانی سطح‌عرض است: باید هم صفحات MVC و هم APIها را با یک منطق واحد بپوشاند و بهتر است قبل از هر پردازش توکن، درخواست نامجاز را قطع کند (Fail-fast). جایگزینِ درست برای محدودسازی «بر هر اکشن مشخص»، Authorization Policy سفارشی است؛ ولی برای «کل ناحیه‌ی محافظت‌شده» Middleware ساده‌تر و خواناتر است. نکته‌ی مهم امنیتی: محدودسازی IP مکمل JWT است نه جایگزین آن — اول IP (کدام کامپیوتر)، بعد توکن (کدام کاربر).

### نکات اجرا و تست
1.  دیتابیس‌های قدیمی : چون دمو از `EnsureCreated` استفاده می‌کند، جدول `AllowedIps` در اولین اجرای نسخه‌ی جدید خودکار ساخته می‌شود (دستور شرطی در Program.cs) — نیازی به پاک‌کردن دیتابیس نیست.
2.  Seed اولیه : اگر جدول خالی باشد، دو IP لوکال‌هاست (`127.0.0.1` و `::1`) با اعتبار ۱۰ ساله ثبت می‌شود — وگرنه صفحه تنظیمات هم از دسترس خارج می‌شد (مسئله مرغ و تخم‌مرغ).
3.  تست رد شدن : در `/Settings` اعتبار IP خودتان را حذف کنید و به `/Person` بروید → صفحه‌ی «دسترسی رد شد» می‌بینید. برای بازگشت: دکمه‌ی افزودن سریع در همان صفحه کار نمی‌کند (چون خود `/Settings/Add` هم محافظت می‌شود)؛ طبق راهنمای همان صفحه، `DELETE FROM AllowedIps;` را در دیتابیس بزنید و برنامه را ری‌استارت کنید.
4.  تست انقضا : یک IP با اعتبار ۲ دقیقه‌ای اضافه کنید؛ بعد از انقضا حداکثر ۳۰ ثانیه بعد (کش) رد شدن را می‌بینید.
5.  پشت Reverse Proxy : اگر از `X-Forwarded-For` استفاده می‌کنید، ابتدا `UseForwardedHeaders` را فعال کنید وگرنه IP واقعی کلاینت دیده نمی‌شود.

## نکات فنی و امنیتی

-  Swashbuckle نسخه‌ی 10.3.1 : مبتنی بر Microsoft.OpenApi 2.x است؛ به همین دلیل الگوی `AddSecurityRequirement` با نسخه‌های قدیمی (۶ تا ۹) فرق دارد — در `Program.cs` از الگوی جدید (`OpenApiSecuritySchemeReference` + delegate) استفاده شده است.
- کلید تنظیم‌شده در `appsettings.json` دقیقاً ۳۲ کاراکتر (۲۵۶ بیت) است و حداقلِ HMAC-SHA256 را برآورده می‌کند (`TokenService.CreateSigningKey` فقط برای کلیدهای کوتاه‌تر، طول را با تکرار جبران می‌کند). در پروژه‌ی واقعی کلید تصادفی ۶۴+ کاراکتری در user-secrets/Key Vault قرار دهید — کلیدِ حاضر داخل مخزن عمومی است و «همه» آن را می‌دانند.
- هش رمز عبور با BCrypt (پکیج BCrypt.Net-Next، Work Factor = 12) انجام می‌شود
- برنامه روی HTTP اجرا می‌شود تا دمو ساده بماند؛ در Production از HTTPS استفاده کنید و `Secure = true` به کوکی‌ها اضافه کنید.
- چرخش توکن (Rotation) هر توکن تمدید را فقط یک‌بار قابل استفاده می‌کند؛ تب‌های موازی ممکن است به ورود هدایت شوند.
- از `EnsureCreated` فقط برای دمو استفاده شده؛ برای پروژه‌ی واقعی: `dotnet ef migrations add Init` سپس `dotnet ef database update`

# جریان واقعی کار
	1- POST api/auth/login	-> { accessToken, refreshToken } -> refresh در جدول RefreshTokens ذخیره می‌شود

	2- هر درخواست بعدی: Authorization: Bearer <accessToken>  -> سرور فقط امضا را چک می‌کند، سریع، بدون دیتابیس

	3- accessToken منقضی شد (مثلاً بعد از ۱۵ دقیقه) -> سرور 401 برمی‌گرداند
 
	4- کلاینت بی‌صدا: POST api/auth/refresh با refreshToken -> { accessToken جدید, refreshToken جدید }   -> کاربر هیچ نفهمید!