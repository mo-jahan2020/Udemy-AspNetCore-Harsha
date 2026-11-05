# پروژه‌ی آموزشی JWT — نسخه‌ی Web API خالص + Swagger

ASP.NET Core 10 · **Web API** (بدون View و صفحه‌ی وب) · **Swagger** · EF Core · SQL Express

بازنویسی پروژه‌ی آموزشی JWT به‌صورت **Web API خالص**: تمام صفحات و Viewهای MVC حذف شده‌اند
و تنها رابط کاربری برنامه، **Swagger UI** در آدرس **/swagger** است. احراز هویت فقط با هدر
استاندارد `Authorization: Bearer ...` انجام می‌شود — نه هیچ کوکی‌ای.

## امکانات

- **معماری Web API خالص**: فقط کنترلرهای attribute-route شده‌ی `[ApiController]` — بدون Razor، بدون View، بدون فایل استاتیک
- **Swagger UI** در آدرس **/swagger** با دکمه‌ی **Authorize** برای تست APIها با توکن JWT (ریشه‌ی `/` هم به همین‌جا هدایت می‌شود)
- **REST API**:
  - `api/auth` — ثبت‌نام، ورود، تمدید توکن (Refresh با چرخش)، خروج
  - `api/persons` — CRUD کامل اشخاص، محافظت‌شده با JWT (`[Authorize]`)
  - `api/settings` — مدیریت فهرست IPهای مجاز (ویژگی IP AllowList) با اعتبارسنجی IP، انقضای دلخواه و ابطال کش Middleware
  - `api/debug/token-check` — ابزار آموزشی «کدام بررسی اعتبارسنجی شکست خورد؟» (فقط Development)
- **احراز هویت JWT خالص**: توکن فقط از هدر `Authorization: Bearer ...` خوانده می‌شود (رفتار پیش‌فرض JwtBearer)؛ تمدید توکن صریح است: در 401 کلاینت خودش `api/auth/refresh` را صدا می‌زند
- الگوی Repository با اینترفیس جنریک + UnitOfWork + EF Core (Code First)
- **IP AllowList**: فهرست IPهای مجاز همراه تاریخ اتمام اعتبار، ذخیره در دیتابیس و اعمال خودکار با یک Middleware قبل از احراز هویت (مدیریتش از `api/settings` در Swagger)
- **هش رمز عبور با BCrypt** (Work Factor 12 — توصیه‌ی OWASP)
- محافظت با `[Authorize]` از پکیج `Microsoft.AspNetCore.Authorization`
- کامنت‌های آموزشی شماره‌گذاری‌شده (گام 1 تا 22) روی بخش‌های JWT

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

3. مرورگر: **http://localhost:7221/swagger** (ریشه‌ی `/` هم به همین‌جا Redirect می‌شود)

> پورت 7221 با مقدار `Issuer` در تنظیمات JWT یکسان است؛ آن را تغییر ندهید.

دیتابیس در اولین اجرا به‌صورت خودکار ساخته و با چند شخص نمونه پر می‌شود (برای سادگی دمو از `EnsureCreated` استفاده شده؛ در پروژه‌ی واقعی از Migration استفاده کنید).

## سناریوی تست JWT در Swagger

1. به **/swagger** بروید.
2. `POST /api/auth/register` یا `POST /api/auth/login` را اجرا کنید → پاسخ شامل `accessToken` و `refreshToken`.
3. روی دکمه‌ی **Authorize** کلیک کنید، مقدار `accessToken` را وارد کنید و Authorize بزنید.
4. `GET /api/persons` را اجرا کنید → پاسخ 200 با لیست اشخاص (هدر `Authorization: Bearer ...` خودکار ارسال می‌شود).
5. **تا انقضای توکن صبر کنید** و دوباره `GET /api/persons` را بزنید → پاسخ **401** (توکن دسترسی منقضی شده).
6. `POST /api/auth/refresh` را با `refreshToken` اجرا کنید → جفت توکن جدید (چرخش — گام 20).
7. توکن جدید را در Authorize وارد کنید و ادامه دهید.
8. `POST /api/auth/logout` با `refreshToken` → ابطال توکن تمدید (گام 22)؛ از این به بعد refresh با 401 جواب می‌دهد.

## ویژگی IP AllowList — IPهای مجاز همراه با تاریخ انقضا (Middleware + api/settings)

«استفاده از JWT» فقط از IPهای مجازِ ثبت‌شده در دیتابیس ممکن است:

- **مدیریت از Swagger**: `GET/POST/PUT/DELETE /api/settings` (نیازمند توکن JWT). آدرس IP به‌صورت خودکار نرمال‌سازی می‌شود، مدت اعتبار به دقیقه مشخص می‌شود و IP تکراری پاسخ **409** می‌گیرد (نه 500).
- **ذخیره در دیتابیس**: جدول `AllowedIps` (با ایندکس یونیک روی IP) + Repository مستقل و اتصال به UnitOfWork.
- **اعمال با Middleware** (`Middleware/IpAllowListMiddleware.cs`) که قبل از احراز هویت در خط لوله قرار دارد:

| | |
|---|---|
| مسیرهای محافظت‌شده | `/api/persons` · `/api/debug` · `/api/settings` |
| مسیرهای مستثنی | `api/auth` · Swagger · ریشه‌ی `/` |
| رفتار رد شدن | پاسخ JSON با کد **403** + نمایش IP فراخوان |
| کارایی | لیست با کش ۳۰ ثانیه‌ای (`IMemoryCache`) خوانده می‌شود؛ با هر Create/Update/Delete در `api/settings` کش بی‌درنگ باطل می‌شود |

### چرا Middleware؟ (جواب سوال طراحی)
محدودسازی IP یک نگرانی سطح‌عرض است: باید کل ناحیه‌ی محافظت‌شده را با یک منطق واحد پوشش دهد و بهتر است قبل از هر پردازش توکن، درخواست نامجاز را قطع کند (Fail-fast). جایگزینِ درست برای محدودسازی «بر هر اکشن مشخص»، Authorization Policy سفارشی است؛ ولی برای «کل ناحیه‌ی محافظت‌شده» Middleware ساده‌تر و خواناتر است. نکته‌ی مهم امنیتی: محدودسازی IP مکمل JWT است نه جایگزین آن — اول IP (کدام کامپیوتر)، بعد توکن (کدام کاربر).

### نکات اجرا و تست
1. **دیتابیس‌های قدیمی**: چون دمو از `EnsureCreated` استفاده می‌کند، جدول `AllowedIps` در اولین اجرای نسخه‌ی جدید خودکار ساخته می‌شود (دستور شرطی در Program.cs) — نیازی به پاک‌کردن دیتابیس نیست.
2. **Seed اولیه**: اگر جدول خالی باشد، دو IP لوکال‌هاست (`127.0.0.1` و `::1`) با اعتبار ۱۰ ساله ثبت می‌شود — وگرنه `api/settings` هم از دسترس خارج می‌شد (مسئله مرغ و تخم‌مرغ).
3. **تست رد شدن**: در `api/settings` اعتبار IP خودتان را حذف کنید و `GET /api/persons` را صدا بزنید → پاسخ **403** با نمایش IP خودتان. برای بازگشت: `DELETE FROM AllowedIps;` را در دیتابیس بزنید و برنامه را ری‌استارت کنید (seed دوباره localhost را ثبت می‌کند).
4. **تست انقضا**: یک IP با اعتبار ۲ دقیقه‌ای اضافه کنید؛ بعد از انقضا حداکثر ۳۰ ثانیه بعد (کش) رد شدن را می‌بینید.
5. **پشت Reverse Proxy**: اگر از `X-Forwarded-For` استفاده می‌کنید، ابتدا `UseForwardedHeaders` را فعال کنید وگرنه IP واقعی کلاینت دیده نمی‌شود.

## نقشه‌ی گام‌های پیاده‌سازی JWT

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
| 11 | Authorization with JWT | خواندن توکن از هدر Authorization (پیش‌فرض JwtBearer — در Web API رویدادی لازم نیست) | `Program.cs` |
| 12 | Authorization with JWT | UseAuthentication و UseAuthorization در خط لوله | `Program.cs` |
| 13 | Authorization with JWT | محافظت با [Authorize] روی کنترلر API | `Controllers/Api/PersonsController.cs` |
| 14 | Authorization with JWT | خواندن هویت کاربر از ClaimsPrincipal | `Controllers/Api/PersonsController.cs` |
| 15 | Refresh Tokens | چرا توکن تمدید؟ (توکن کوتاه‌عمر + تمدید ۶۰ دقیقه‌ای) | `Services/TokenService.cs` |
| 16 | Refresh Tokens | تولید مقدار تصادفی امن با RandomNumberGenerator | `Services/TokenService.cs` |
| 17 | Refresh Tokens | ذخیره‌ی «هش» توکن تمدید در دیتابیس با تاریخ انقضا | `Services/AuthService.cs` |
| 18 | Refresh Tokens | ~~کوکی HttpOnly~~ — **در نسخه‌ی Web API حذف شد** (کوکی فقط برای صفحات MVC معنا داشت) | — |
| 19 | Refresh Tokens | اعتبارسنجی توکن تمدید (جستجو در دیتابیس، انقضا، ابطال) | `Services/AuthService.cs` |
| 20 | Refresh Tokens | چرخش توکن (Rotation): صدور جفت توکن جدید و باطل کردن قبلی | `Services/AuthService.cs` |
| 21 | Refresh Tokens | ~~تمدید خودکار در ناوبری صفحات~~ — **در نسخه‌ی Web API حذف شد**؛ کلاینت در 401 خودش `api/auth/refresh` را صدا می‌زند | `Controllers/Api/AuthController.cs` |
| 22 | Refresh Tokens | خروج: ابطال توکن تمدید در دیتابیس | `Services/AuthService.cs` |

> بخش Swagger در `Program.cs` جزو پیاده‌سازی JWT نیست و کامنت شماره‌دار ندارد؛ فقط در `api/auth` و `api/persons` به گام‌های مرتبط ارجاع داده شده است.

## ساختار پروژه

```
JwtDemoMVC/
├── Program.cs                     ← گام‌های 9 تا 12 + پیکربندی Web API و Swagger
├── appsettings.json               ← تنظیمات Jwt و RefreshToken
├── Models/
│   ├── Person.cs                  ← مدل دامنه
│   ├── User.cs                    ← کاربر (هش BCrypt رمز عبور)
│   ├── RefreshToken.cs            ← توکن تمدید (هش، انقضا، ابطال)
│   ├── AllowedIp.cs               ← IP مجاز (تاریخ انقضا) — ویژگی IP AllowList
│   └── ApiContracts.cs            ← مدل‌های درخواست API (بدنه‌های Swagger)
├── Data/AppDbContext.cs
├── Repositories/                  ← الگوی Repository با اینترفیس + UnitOfWork
├── Middleware/
│   └── IpAllowListMiddleware.cs   ← ویژگی IP AllowList (قبل از احراز هویت)
├── Security/PasswordHasher.cs     ← BCrypt (Work Factor 12)
├── Services/
│   ├── TokenService.cs            ← گام‌های 1 تا 8 و 15 و 16
│   └── AuthService.cs             ← گام‌های 17 و 19 و 20 و 22 (فقط متدهای API — بدون کوکی)
└── Controllers/Api/
    ├── AuthController.cs          ← api/auth (ثبت‌نام، ورود، تمدید، خروج)
    ├── PersonsController.cs       ← api/persons (گام‌های 13 و 14 — CRUD محافظت‌شده با JWT)
    ├── SettingsController.cs      ← api/settings (مدیریت IP AllowList)
    └── DebugController.cs         ← api/debug/token-check (فقط Development)
```

> در نسخه‌ی MVC پوشه‌های `Views/` و `ViewModels/` و کنترلرهای `HomeController`، `AccountController`، `PersonController` و `SettingsController` (نسخه‌ی صفحه‌دار) هم وجود داشتند — در این نسخه حذف شده‌اند.

## چرا تغییر Issuer توکن‌ها را باطل نمی‌کند و Audience مانع استفاده‌ی بقیه نمی‌شود؟ (دو اشتباه رایج)

### اشتباه ۱ — «ValidateIssuer باید جلوی ساختنِ توکن را بگیرد»
`ValidateIssuer` فقط «توکنِ رسیده» را بررسی می‌کند، نه فرآیند تولید را. ساختنِ رشته‌ی JWT هیچ رازی لازم ندارد (دو بخش اول توکن فقط Base64 هستند)؛ هر کسی می‌تواند توکنی با هر iss/aud دلخواه بسازد. چیزی که جعل را بی‌فایده می‌کند «امضای دیجیتال» است (`ValidateIssuerSigningKey`): توکنِ امضاشده با کلید اشتباه — حتی با iss/aud کاملاً درست — با 401 رد می‌شود.

پس چرا با تغییر Issuer به 7222 توکن‌های Swagger همچنان معتبر ماندند؟ چون `TokenService` (تولید) و `Program.cs` (اعتبارسنجی) **هر دو از همان یک مقدار appsettings می‌خوانند**؛ تغییر عدد، هر دو سمت را با هم عوض می‌کند. برای دیدن رد شدنِ واقعی: سرور را روی 7222 نگه دارید و توکنی با iss=7221 (با همان کلید) بفرستید → 401 با `IDX10214: Issuer validation failed`.

### اشتباه ۲ — «ValidAudience یعنی فقط کلاینتِ localhost:4200 مجاز است»
`aud` یک ادعای *داخل توکن* است، نه آدرس فراخوان؛ سرور هیچ‌وقت آدرس/Origin فرستنده را با aud مقایسه نمی‌کند. JWT از نوع **Bearer** است: هر کس توکن را «داشته باشد» تا انقضا می‌تواند استفاده کند — چه از localhost:4200 چه 7221. کاربرد واقعی aud جلوگیری از استفاده‌ی توکنِ یک API در APIی دیگر است (توکنِ aud=A در سروری که ValidAudience=B دارد رد می‌شود). برای محدودکردن مرورگرها CORS و برای محدودکردن کلاینت‌ها IP Allowlist/mTLS به‌کار می‌رود، نه aud.

### چهار آزمایش چند‌دقیقه‌ای (بدون تغییر کد)
کلید این دمو دقیقاً ۳۲ کاراکتر است، پس می‌توانید در [jwt.io](https://jwt.io) با همان کلیدِ `appsettings.json` توکن دستی بسازید و از دکمه‌ی Authorize به سرور بفرستید:

| # | تغییر در jwt.io | نتیجه در سرور | خطا |
|---|------------------|----------------|------|
| 1 | iss → `http://localhost:9999` | 401 | IDX10214 (Issuer) |
| 2 | aud → `http://localhost:9999` | 401 | IDX10215 (Audience) |
| 3 | یک کاراکتر از secret کلید | 401 | IDX10603 (Signature) — دیوار واقعی جلوی جعل |
| 4 | exp در گذشته | 401 | IDX10223 (Expired) |

- دلیل دقیق هر 401 در **لاگ کنسول** برنامه هم چاپ می‌شود (کد IDX).
- برای دیدن گزارش ساخت‌یافته‌ی «کدام بررسی شکست خورد»: `POST /api/debug/token-check` با بدنه‌ی `{ "token": "..." }` (فقط در Development فعال است).

### نکته‌ی امنیتی مهم‌تر از هر دو مورد
چون کلید امضا داخل `appsettings.json` و داخل یک مخزن **عمومی** است، هر کسی که این مخزن را دیده باشد می‌تواند توکنِ کاملاً معتبر (با iss/aud درست) بسازد — این محدودیت دموی آموزشی است، نه نقص ValidateIssuer. در پروژه‌ی واقعی: کلید ۶۴+ کاراکتری تصادفی در user-secrets/متغیر محیطی/Key Vault، و کلید را هرگز Commit نکنید.

## نکات فنی و امنیتی

- **Swashbuckle نسخه‌ی 10.3.1**: مبتنی بر Microsoft.OpenApi 2.x است؛ به همین دلیل الگوی `AddSecurityRequirement` با نسخه‌های قدیمی (۶ تا ۹) فرق دارد — در `Program.cs` از الگوی جدید (`OpenApiSecuritySchemeReference` + delegate) استفاده شده است.
- کلید تنظیم‌شده در `appsettings.json` دقیقاً ۳۲ کاراکتر (۲۵۶ بیت) است و حداقلِ HMAC-SHA256 را برآورده می‌کند (`TokenService.CreateSigningKey` فقط برای کلیدهای کوتاه‌تر، طول را با تکرار جبران می‌کند). در پروژه‌ی واقعی کلید تصادفی ۶۴+ کاراکتری در user-secrets/Key Vault قرار دهید — کلیدِ حاضر داخل مخزن عمومی است و «همه» آن را می‌دانند.
- **هش رمز عبور با BCrypt** (پکیج BCrypt.Net-Next، Work Factor 12): نمک تصادفی داخل خود رشته‌ی هش جای می‌گیرد (فیلد جداگانه‌ای برای نمک لازم نیست) و `Verify` حتی با هش خراب هم به‌صورت امن false برمی‌گرداند.
- برنامه روی HTTP اجرا می‌شود تا دمو ساده بماند؛ در Production از HTTPS استفاده کنید.
- چرخش توکن (Rotation) هر توکن تمدید را فقط یک‌بار قابل استفاده می‌کند؛ کلاینت‌هایی که توکن تمدید قدیمی را نگه دارند در فراخوانی بعدی 401 می‌گیرند.
- از `EnsureCreated` فقط برای دمو استفاده شده؛ برای پروژه‌ی واقعی: `dotnet ef migrations add Init` سپس `dotnet ef database update`
