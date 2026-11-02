// ═══════════════════════════════════════════════════════════════════════════
//  AuthService.cs — هماهنگی توکن‌ها با دیتابیس و کوکی‌ها
//  گام‌های این فایل: 17، 18، 19، 20، 22 (همه از مجموعه‌ی Refresh Tokens)
//  سایر گام‌ها: TokenService.cs (1 تا 8 و 15 و 16) — Program.cs (9 تا 12 و 21)
//               Persons.cshtml.cs (13 و 14)
// ═══════════════════════════════════════════════════════════════════════════

using JwtDemo.Models;
using JwtDemo.Repositories;

namespace JwtDemo.Services;

public class AuthService : IAuthService
{
    // نام کوکی‌هایی که توکن‌ها در آن‌ها نگه‌داری می‌شوند (گام 18)
    public const string AccessTokenCookie = "jwt";
    public const string RefreshTokenCookie = "refreshToken";

    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    private int AccessTokenMinutes => int.Parse(_configuration["Jwt:EXPIRATION_MINUTES"] ?? "1");
    private int RefreshTokenMinutes => int.Parse(_configuration["RefreshToken:EXPIRATION_MINUTES"] ?? "60");

    public async Task IssueTokensAsync(HttpContext context, User user)
    {
        // تولید توکن دسترسی JWT — گام‌های 4 تا 8 داخل TokenService اجرا می‌شوند
        var accessToken = _tokenService.GenerateAccessToken(user);

        // تولید توکن تمدید — گام 16
        var refreshToken = _tokenService.GenerateRefreshToken();

        // ═══════════ گام 17 | Refresh Tokens: ذخیره‌ی توکن تمدید در دیتابیس ═══════════
        // برخلاف توکن دسترسی (که Stateless است)، توکن تمدید در دیتابیس نگه‌داری می‌شود
        // تا بتوان انقضا و ابطال آن را کنترل کرد. فقط «هش» آن ذخیره می‌شود تا اگر
        // دیتابیس لو برود، توکن‌های واقعی قابل استفاده نباشند.
        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            TokenHash = _tokenService.HashToken(refreshToken),
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(RefreshTokenMinutes), // طبق تنظیمات: 60 دقیقه
        });
        await _unitOfWork.SaveChangesAsync();

        // ═══════════ گام 18 | Refresh Tokens: قرار دادن توکن‌ها در کوکی‌های HttpOnly ═══════════
        // در برنامه‌های API توکن به کلاینت داده می‌شود تا در هدر Authorization بفرستد؛
        // اما در Razor Pages مرورگر هنگام ناوبری بین صفحات نمی‌تواند هدر Authorization بسازد،
        // پس توکن‌ها را در کوکی نگه می‌داریم. HttpOnly یعنی جاوااسکریپت به کوکی دسترسی
        // ندارد → محافظت در برابر سرقت توکن با XSS.
        // انقضای هر کوکی را دقیقاً همگام با انقضای خودِ توکن تنظیم می‌کنیم تا مرورگر
        // خودش توکن‌های منقضی‌شده را حذف کند.
        SetTokenCookies(context, accessToken, refreshToken);
    }

    public async Task<string?> TryRefreshAsync(HttpContext context)
    {
        // ═══════════ گام 19 | Refresh Tokens: اعتبارسنجی توکن تمدید ═══════════
        // توکن تمدید از کوکی خوانده، هش و در دیتابیس جستجو می‌شود.
        var rawRefreshToken = context.Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(rawRefreshToken))
            return null; // توکن تمدیدی وجود ندارد → کاربر باید مجدداً وارد شود

        var storedToken = await _unitOfWork.RefreshTokens
            .GetByTokenHashAsync(_tokenService.HashToken(rawRefreshToken));

        if (storedToken is null || !storedToken.IsActive)
            return null; // توکن تمدید نامعتبر است یا منقضی شده (60 دقیقه) یا قبلاً باطل شده است

        // ═══════════ گام 20 | Refresh Tokens: چرخش توکن (Rotation) ═══════════
        // سیاست امنیتی مهم: هر بار تمدید، «جفتِ توکن کاملاً جدید» صادر می‌شود و
        // توکن‌های قبلی باطل می‌شوند. این‌طور اگر توکنی لو برود، مالک واقعی با اولین
        // استفاده‌ی بعدی آن را باطل می‌کند و مهاجم از کار می‌افتد.
        var newAccessToken = _tokenService.GenerateAccessToken(storedToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // باطل کردن توکن تمدید قبلی و ثبت هش توکن جایگزین (زنجیره‌ی چرخش)
        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = _tokenService.HashToken(newRefreshToken);

        // ذخیره‌ی توکن تمدید جدید در دیتابیس (گام 17)
        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            TokenHash = _tokenService.HashToken(newRefreshToken),
            UserId = storedToken.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(RefreshTokenMinutes),
        });
        await _unitOfWork.SaveChangesAsync();

        // تنظیم مجدد کوکی‌ها با توکن‌های جدید (گام 18)
        SetTokenCookies(context, newAccessToken, newRefreshToken);

        // برای نمایش توکن جدید در «پنل آموزشی JWT» در همین درخواست (پشتیبانِ گام 21) —
        // چون کوکی درخواستِ جاری هنوز مقدار قبلی را دارد.
        context.Items["CurrentAccessToken"] = newAccessToken;
        context.Items["CurrentRefreshToken"] = newRefreshToken;

        return newAccessToken;
    }

    public async Task RevokeAsync(HttpContext context)
    {
        // ═══════════ گام 22 | Refresh Tokens: خروج و ابطال ═══════════
        // JWT ذاتاً «بدون حالت» (Stateless) است؛ سرور نمی‌تواند توکن دسترسی را از راه دور
        // باطل کند و تا پایان عمرش (فقط 1 دقیقه) معتبر می‌ماند. آنچه در خروج واقعاً انجام می‌دهیم:
        //   1) ابطال توکن تمدید در دیتابیس → بعد از انقضای توکن دسترسی، دیگر قابل تمدید نیست
        //      و مسیر دسترسی کاربر به‌کلی بسته می‌شود.
        //   2) حذف کوکی‌ها از مرورگر → توکن دسترسی هم از دسترس خارج می‌شود.
        var rawRefreshToken = context.Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(rawRefreshToken))
        {
            var storedToken = await _unitOfWork.RefreshTokens
                .GetByTokenHashAsync(_tokenService.HashToken(rawRefreshToken));

            if (storedToken is { IsActive: true })
            {
                storedToken.RevokedAtUtc = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        context.Response.Cookies.Delete(AccessTokenCookie);
        context.Response.Cookies.Delete(RefreshTokenCookie);
    }

    // تنظیم کوکی‌های HttpOnly برای هر دو توکن (گام 18)
    private void SetTokenCookies(HttpContext context, string accessToken, string refreshToken)
    {
        context.Response.Cookies.Append(AccessTokenCookie, accessToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AccessTokenMinutes), // همگام با عمر توکن: 1 دقیقه
        });

        context.Response.Cookies.Append(RefreshTokenCookie, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(RefreshTokenMinutes), // همگام با عمر توکن تمدید: 60 دقیقه
        });
    }
}
