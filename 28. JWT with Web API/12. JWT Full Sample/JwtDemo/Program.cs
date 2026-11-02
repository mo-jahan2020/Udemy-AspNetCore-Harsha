// ═══════════════════════════════════════════════════════════════════════════
//  Program.cs — راه‌اندازی برنامه
//  گام‌های این فایل: 9، 10، 11، 12 (Authorization with JWT) + 21 (Refresh Tokens)
//  سایر گام‌ها: TokenService.cs (1 تا 8 و 15 و 16) — AuthService.cs (17 تا 20 و 22)
//               Persons.cshtml.cs (13 و 14)
// ═══════════════════════════════════════════════════════════════════════════

using JwtDemo.Data;
using JwtDemo.Models;
using JwtDemo.Repositories;
using JwtDemo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// اتصال به SQL Server Express با Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// ثبت Repositoryها و UnitOfWork (الگوی Repository با اینترفیس)
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// سرویس‌های توکن و احراز هویت
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ═══════════ گام 9 | Authorization with JWT: ثبت سرویس احراز هویت JWT ═══════════
// پیش‌فرض برنامه روی احراز هویت «JWT Bearer» قرار می‌گیرد؛ یعنی هر درخواست باید
// توکن معتبر JWT همراه خودش داشته باشد: در هدر Authorization (برای APIها)
// یا در کوکی (برای ناوبری صفحات Razor — گام 11).
// ویژگی [Authorize] که در صفحه‌ی اشخاص استفاده شده (گام 13) دقیقاً روی همین
// مکانیزم تکیه می‌کند و از پکیج Microsoft.AspNetCore.Authorization است.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // نگاشت نام Claims استاندارد JWT (مثل unique_name) به ClaimTypes دات‌نت
        // تا User.Identity.Name و User.Claims به درستی پر شوند (گام 14)
        options.MapInboundClaims = true;

        // ═══════════ گام 10 | Authorization with JWT: معیارهای اعتبارسنجی توکن ═══════════
        // سرور با این پارامترها تشخیص می‌دهد توکنِ دریافتی «معتبر» است یا جعلی/منقضی:
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
        };

        options.Events = new JwtBearerEvents
        {
            // ═══════════ گام 11 | Authorization with JWT: خواندن توکن از کوکی ═══════════
            // در APIها توکن JWT در هدر «Authorization: Bearer ...» ارسال می‌شود؛ اما مرورگر
            // هنگام ناوبری بین صفحات Razor چنین هدری نمی‌فرستد. پس توکن را در یک کوکی
            // HttpOnly نگه می‌داریم و اینجا به JwtBearer می‌گوییم آن را از کوکی بخواند.
            OnMessageReceived = async context =>
            {
                // حالت اول: توکن از هدر Authorization (مثل یک کلاینت API) — رفتار پیش‌فرض JWT
                var header = context.Request.Headers.Authorization.ToString();
                if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = header["Bearer ".Length..].Trim();
                    return;
                }

                // حالت دوم: توکن از کوکی (ناوبری صفحات Razor)
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

            // کاربرِ احراز هویت‌نشده به صفحه‌ی ورود هدایت شود (به‌جای پاسخ خام 401)
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Login");
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

    if (!db.Persons.Any())
    {
        db.Persons.AddRange(
            new Person { FirstName = "علی", LastName = "رضایی", Age = 28 },
            new Person { FirstName = "سارا", LastName = "احمدی", Age = 34 },
            new Person { FirstName = "محمد", LastName = "کریمی", Age = 19 });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// فایل‌های استاتیک (CSS آفلاین) — قبل از احراز هویت تا برایشان توکن لازم نباشد
app.UseStaticFiles();

// ═══════════ گام 12 | Authorization with JWT: قرارگیری Middlewareها در خط لوله ═══════════
// ترتیب بسیار مهم است:
//   1) UseAuthentication → توکن JWT خوانده و اعتبارسنجی می‌شود و HttpContext.User پر می‌شود
//   2) UseAuthorization  → بر اساس نقش‌ها/سیاست‌ها و ویژگی [Authorize] تصمیم گیری می‌شود
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
