// ═══════════════════════════════════════════════════════════════════════════
//  Program.cs — راه‌اندازی برنامه (نسخه‌ی MVC + Swagger)
//  گام‌های این فایل: 9، 10، 11، 12 (Authorization with JWT) + 21 (Refresh Tokens)
//  سایر گام‌ها: TokenService.cs (1 تا 8 و 15 و 16) — AuthService.cs (17 تا 20 و 22)
//               Controllers/PersonController.cs (13 و 14)
//  تفاوت با نسخه‌ی Razor Pages:
//   • AddControllersWithViews به‌جای AddRazorPages و MapDefaultControllerRoute
//   • افزودن Swagger (آدرس /swagger) برای تست تعاملی APIها با توکن JWT
// ═══════════════════════════════════════════════════════════════════════════

using JwtDemoMVC.Data;
using JwtDemoMVC.Middleware;
using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using JwtDemoMVC.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ═══ MVC: کنترلرها + Viewها ═══
builder.Services.AddControllersWithViews();

// کش در حافظه — Middleware IP AllowList لیست IPهای مجاز را با این کش می‌خواند
// تا هر درخواست به دیتابیس نخورد (با هر تغییر در صفحه تنظیمات، کش باطل می‌شود)
builder.Services.AddMemoryCache();

// اتصال به SQL Server Express با Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// ثبت Repositoryها و UnitOfWork (الگوی Repository با اینترفیس)
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
// ویژگی IP AllowList — فهرست IPهای مجاز در دیتابیس (صفحه تنظیمات آن را مدیریت می‌کند)
builder.Services.AddScoped<IAllowedIpRepository, AllowedIpRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// سرویس‌های توکن و احراز هویت
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ═══════════════════════════════════════════════════════════════════════════
//  Swagger — مستندسازی و تست تعاملی API در آدرس /swagger
//  (این بخش جزو پیاده‌سازی JWT نیست و کامنت شماره‌گذاری‌شده ندارد)
// ═══════════════════════════════════════════════════════════════════════════
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API آموزشی JWT",
        Version = "v1",
        Description = "ابتدا در api/auth/register یا api/auth/login توکن بگیرید، سپس با دکمه‌ی Authorize " +
                      "توکن را وارد کنید و Endpointهای محافظت‌شده (قفل‌دار) را تست کنید. " +
                      "عمر توکن دسترسی فقط 1 دقیقه است؛ با api/auth/refresh آن را تمدید کنید."
    });

    // تعریف طرح امنیتی Bearer — باعث ظاهر شدن دکمه‌ی Authorize در Swagger UI می‌شود.
    // با این کار می‌توان توکن JWT را داخل Swagger وارد کرد تا هدر
    // «Authorization: Bearer ...» به‌صورت خودکار به همه‌ی درخواست‌ها اضافه شود.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "توکن دسترسی JWT را اینجا وارد کنید (نیازی به نوشتن کلمه‌ی Bearer نیست)"
    });

    // اعمال طرح امنیتی روی کل سند OpenAPI.
    // نکته: در Swashbuckle نسخه‌ی 10 (مبتنی بر Microsoft.OpenApi 2.x) امضای این متد
    // تغییر کرده است: به‌جای OpenApiSecurityScheme.Reference حذف‌شده، از
    // OpenApiSecuritySchemeReference استفاده می‌شود و ورودی، یک delegate است که
    // سند جاری (document) را دریافت می‌کند.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
        new OpenApiSecuritySchemeReference("Bearer" , document),
        new List<string>()
        }

    });

    // نمایش کامنت‌های XML (///) روی هر اکشن، به‌عنوان توضیح در Swagger UI
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ═══════════ گام 9 | Authorization with JWT: ثبت سرویس احراز هویت JWT ═══════════
// پیش‌فرض برنامه روی احراز هویت «JWT Bearer» قرار می‌گیرد؛ یعنی هر درخواست باید
// توکن معتبر JWT همراه خودش داشته باشد: در هدر Authorization (برای APIها و Swagger)
// یا در کوکی (برای ناوبری صفحات MVC — گام 11).
// ویژگی [Authorize] که روی کنترلر اشخاص و API اشخاص استفاده شده (گام 13) دقیقاً
// روی همین مکانیزم تکیه می‌کند و از پکیج Microsoft.AspNetCore.Authorization است.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // نگاشت نام Claims استاندارد JWT (مثل unique_name) به ClaimTypes دات‌نت
        // تا User.Identity.Name و User.Claims به درستی پر شوند (گام 14)
        options.MapInboundClaims = true;

        // ═══════════ گام 10 | Authorization with JWT: معیارهای اعتبارسنجی توکن ═══════════
        // سرور با این پارامترها تشخیص می‌دهد توکنِ دریافتی «معتبر» است یا جعلی/منقضی:
        //
        // ⚠️ سه اشتباه رایج که این پارامترها «برطرف نمی‌کنند» (و قرار هم نیست برطرف کنند):
        //
        //  ۱) ValidateIssuer جلوی «ساختنِ» توکن را نمی‌گیرد؛ فقط «توکنِ رسیده» را بررسی می‌کند.
        //     ساختن رشته‌ی JWT هیچ رازی لازم ندارد (دو بخش اول فقط Base64 هستند)؛ هر کسی
        //     می‌تواند توکنی با هر iss/aud دلخواه بسازد. آنچه جعل را بی‌فایده می‌کند «امضای
        //     دیجیتال» است (ValidateIssuerSigningKey پایین) — توکنِ امضاشده با کلید اشتباه،
        //     حتی با iss/aud کاملاً درست، با 401 رد می‌شود.
        //
        //  ۲) چون TokenService (تولید) و همین‌جا (اعتبارسنجی) هر دو از «همان یک مقدار appsettings»
        //     می‌خوانند، تغییر Issuer از 7221 به 7222 هر دو سمت را با هم عوض می‌کند و توکن‌های
        //     خود برنامه همچنان معتبر می‌مانند. برای دیدن رد شدنِ واقعی: سرور روی 7222 بماند و
        //     توکنی با iss=7221 (با همان کلید) بفرستید → 401 با خطای IDX10214.
        //
        //  ۳) ValidateAudience یعنی «ادعای aud داخل توکن» باید برابر ValidAudience باشد؛
        //     ربطی به آدرس/Origin فراخوان ندارد. JWT از نوع Bearer است: هر کس توکن را داشته
        //     باشد تا انقضا می‌تواند از آن استفاده کند؛ aud فقط جلوی استفاده‌ی توکنِ یک API
        //     در API دیگر را می‌گیرد (توکنِ aud=A در سروری که ValidAudience=B دارد رد می‌شود).
        //
        //  گزارش «کدام بررسی شکست خورد»: POST /api/debug/token-check (فقط Development)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // صادرکننده‌ی توکن باید دقیقاً Issuer تنظیمات باشد (گام 3)
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            // مصرف‌کننده‌ی توکن باید دقیقاً Audience تنظیمات باشد
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // تاریخ انقضای توکن بررسی شود (توکن 1 دقیقه‌ای!)
            ValidateLifetime = true,

            // مهم: پیش‌فرض ClockSkew پنج دقیقه تحمل زمانی دارد که دموی 1 دقیقه‌ای را
            // خراب می‌کند (توکن تا 6 دقیقه معتبر می‌ماند!) → صفرش می‌کنیم.
            ClockSkew = TimeSpan.Zero,

            // مهم‌ترین بخش: امضای دیجیتال توکن با همان کلیدِ رمزی که در گام 4
            // توکن با آن امضا شده بود، بررسی می‌شود. اگر حتی یک کاراکتر از توکن
            // دستکاری شده باشد، امضا معتبر نخواهد بود.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TokenService.CreateSigningKey(builder.Configuration["Jwt:Key"]!),
            // ⚠️ همین کلید «تنها محافظت واقعی در برابر جعل» است؛ اگر لو برود (مثل این دمو که
            // داخل مخزن عمومی گیت‌هاب است)، هر کسی می‌تواند توکنِ کاملاً معتبر بسازد —
            // با هر Issuer/Audience دلخواه! در پروژه‌ی واقعی: user-secrets / متغیر محیطی / Key Vault.
        };

        options.Events = new JwtBearerEvents
        {
            // ═══════════ گام 11 | Authorization with JWT: خواندن توکن از کوکی یا هدر ═══════════
            // در APIها (و Swagger) توکن JWT در هدر «Authorization: Bearer ...» ارسال می‌شود؛
            // اما مرورگر هنگام ناوبری بین صفحات MVC چنین هدری نمی‌فرستد؛ پس توکن را در یک
            // کوکی HttpOnly نگه می‌داریم و اینجا به JwtBearer می‌گوییم آن را از کوکی بخواند.
            OnMessageReceived = async context =>
            {
                // حالت اول: توکن از هدر Authorization (کلاینت‌های API مثل Swagger)
                var header = context.Request.Headers.Authorization.ToString();
                if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = header["Bearer ".Length..].Trim();
                    return;
                }

                // حالت دوم: توکن از کوکی (ناوبری صفحات MVC)
                var token = context.Request.Cookies[AuthService.AccessTokenCookie];

                // ═══════════ گام 21 | Refresh Tokens: تمدید خودکار توکن ═══════════
                // اگر توکن دسترسی منقضی شده باشد (یا کوکی‌اش توسط مرورگر حذف شده باشد)،
                // به صورت خودکار و بی‌صدا با «توکن تمدید» موجود، جفت توکن جدید صادر می‌شود
                // (گام‌های 19 و 20 در AuthService) — کاربر متوجه هیچ چیزی نمی‌شود!
                // اگر توکن تمدید هم معتبر نباشد، همان توکن منقضی می‌ماند و چالش (Challenge)
                // کاربر را به صفحه‌ی ورود می‌فرستد.
                if (string.IsNullOrEmpty(token) || TokenService.IsTokenExpired(token))
                {
                    var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                    token = await authService.TryRefreshAsync(context.HttpContext) ?? token;
                }

                context.Token = token;
            },

            // رفتار چالش (دسترسی غیرمجاز):
            //  • در API (مسیر /api) → همان پاسخ استاندارد 401 با هدر WWW-Authenticate
            //    (Swagger هم همین را انتظار دارد)
            //  • در صفحات MVC → کاربر به صفحه‌ی ورود هدایت می‌شود (به‌جای 401 خام)
            OnChallenge = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                    return Task.CompletedTask; // رفتار پیش‌فرض JwtBearer اجرا شود

                context.HandleResponse();
                // در غیر اینصورت چون توکن تهی است باید وارد شود
                context.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ایجاد خودکار دیتابیس در اولین اجرا + داده‌ی نمونه
// (برای سادگی دمو از EnsureCreated استفاده شده؛ در پروژه واقعی از Migration استفاده کنید)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // ویژگی IP AllowList: اگر دیتابیس از اجرای قبلی (نسخه‌ی قبل) از پیش ساخته شده باشد،
    // EnsureCreated جدول جدید AllowedIps را نمی‌سازد؛ پس در دمو دستی می‌سازیم.
    // در پروژه‌ی واقعی: dotnet ef migrations add AddAllowedIps → database update
    db.Database.ExecuteSqlRaw("""
        IF OBJECT_ID(N'dbo.AllowedIps', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[AllowedIps] (
                [Id] int NOT NULL IDENTITY,
                [IpAddress] nvarchar(45) NOT NULL,
                [Note] nvarchar(200) NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [ExpiresAtUtc] datetime2 NOT NULL,
                CONSTRAINT [PK_AllowedIps] PRIMARY KEY ([Id])
            );
            CREATE UNIQUE INDEX [IX_AllowedIps_IpAddress] ON [dbo].[AllowedIps] ([IpAddress]);
        END
        """);

    if (!db.Persons.Any())
    {
        db.Persons.AddRange(
            new Person { FirstName = "علی", LastName = "رضایی", Age = 28 },
            new Person { FirstName = "سارا", LastName = "احمدی", Age = 34 },
            new Person { FirstName = "محمد", LastName = "کریمی", Age = 19 });
        db.SaveChanges();
    }

    // Seed اولیه‌ی IPهای مجاز: localhost همیشه مجاز است — حل مساله‌ی مرغ و تخم‌مرغ:
    // برای افزودن IP باید به صفحه تنظیمات برسی، و برای رسیدن به صفحه تنظیمات، IP باید مجاز باشد!
    // (اگر همه‌ی رکوردها را حذف کرده‌اید، ری‌استارت کنید تا این seed دوباره اجرا شود)
    if (!db.AllowedIps.Any())
    {
        db.AllowedIps.AddRange(
            new AllowedIp
            {
                IpAddress = "127.0.0.1",
                Note = "seed خودکار — localhost (IPv4)",
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddYears(10),
            },
            new AllowedIp
            {
                IpAddress = "::1",
                Note = "seed خودکار — localhost (IPv6)",
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddYears(10),
            });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// فایل‌های استاتیک (CSS آفلاین) — قبل از احراز هویت تا برایشان توکن لازم نباشد
app.UseStaticFiles();

// Swagger UI — در همه‌ی محیط‌ها فعال (دموی آموزشی) → آدرس: /swagger
// نکته: فایل‌های Swagger UI داخل خود پکیج قرار دارند (بدون هیچ CDN آنلاین)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API آموزشی JWT v1");
    options.DocumentTitle = "API آموزشی JWT";
});

// ═══════════ ویژگی IP AllowList: بررسی IPهای مجاز قبل از احراز هویت ═══════════
// فهرست مجازها در جدول AllowedIps (دیتابیس) است و صفحه‌ی /Settings آن را مدیریت می‌کند.
// جایگاه در خط لوله: بعد از UseStaticFiles و Swagger (تا عمومی بمانند) و
// قبل از UseAuthentication (تا درخواست IP نامجاز قبل از هر پردازش توکن قطع شود):
//   اول «کامپیوترِ فراخوان» بررسی می‌شود، بعد «توکنِ کاربر».
// رفتار رد شدن: مسیرهای /api → 403 JSON؛ صفحات MVC → ریدایرکت به صفحه‌ی /Settings/Denied
app.UseMiddleware<IpAllowListMiddleware>();

// ═══════════ گام 12 | Authorization with JWT: قرارگیری Middlewareها در خط لوله ═══════════
// ترتیب بسیار مهم است:
//   1) UseAuthentication → توکن JWT خوانده و اعتبارسنجی می‌شود و HttpContext.User پر می‌شود
//   2) UseAuthorization  → بر اساس نقش‌ها/سیاست‌ها و ویژگی [Authorize] تصمیم‌گیری می‌شود
app.UseAuthentication();
app.UseAuthorization();

// MVC: مسیریابی پیش‌فرض  {controller=Home}/{action=Index}/{id?}
app.MapDefaultControllerRoute();

app.Run();
