// ═══════════════════════════════════════════════════════════════════════════
//  AuthService.cs — هماهنگی توکن‌ها با دیتابیس (نسخه‌ی Web API خالص)
//  گام‌های این فایل: 17، 19، 20، 22 (از مجموعه‌ی Refresh Tokens)
//  تمام متدها «خام» (بدون کوکی) هستند: توکن‌ها در پاسخ JSON به کلاینت
//  برمی‌گردند و کلاینت خودش آن‌ها را نگه می‌دارد و در api/auth/refresh می‌فرستد.
//  (در نسخه‌ی MVC، متدهای کوکی‌محور IssueTokensAsync/TryRefreshAsync/RevokeAsync
//   و گام 18 کوکی HttpOnly و گام 21 تمدید خودکار اینجا بودند — با حذف صفحات حذف شدند.)
//  سایر گام‌ها: TokenService.cs (1 تا 8 و 15 و 16) — Program.cs (9 تا 12)
//               Controllers/Api/PersonsController.cs (13 و 14)
// ═══════════════════════════════════════════════════════════════════════════

using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;

namespace JwtDemoMVC.Services;

public class AuthService : IAuthService
{
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

    // ─────────────────────────────────────────────────────────────────────────
    //  هسته‌ی مشترک: تولید جفت توکن + ذخیره‌ی توکن تمدید در دیتابیس (گام 17)
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<TokenPair> CreateAndStoreTokensAsync(User user)
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

        return new TokenPair(
            accessToken,
            refreshToken,
            AccessTokenExpiresAtUtc: DateTime.UtcNow.AddMinutes(AccessTokenMinutes),
            RefreshTokenExpiresAtUtc: DateTime.UtcNow.AddMinutes(RefreshTokenMinutes));
    }

    // ══════════════════════════ برای کلاینت‌های API و Swagger ══════════════════════════

    // صدور جفت توکن بدون کوکی — توکن‌ها در پاسخ JSON (TokenPair) برگردانده می‌شوند (گام 17)
    public async Task<TokenPair> IssueTokensForApiAsync(User user)
        => await CreateAndStoreTokensAsync(user);

    // ═══════════ گام 19 | Refresh Tokens: اعتبارسنجی توکن تمدید ═══════════
    // توکن تمدید هش و در دیتابیس جستجو می‌شود؛ باید وجود داشته باشد، منقضی نشده باشد
    // (60 دقیقه) و قبلاً باطل نشده باشد.
    //
    // ═══════════ گام 20 | Refresh Tokens: چرخش توکن (Rotation) ═══════════
    // سیاست امنیتی مهم: هر بار تمدید، «جفتِ توکن کاملاً جدید» صادر می‌شود و
    // توکن‌های قبلی باطل می‌شوند. این‌طور اگر توکنی لو برود، مالک واقعی با اولین
    // استفاده‌ی بعدی آن را باطل می‌کند و مهاجم از کار می‌افتد.
    public async Task<TokenPair?> RefreshTokenAsync(string rawRefreshToken)
    {
        if (string.IsNullOrEmpty(rawRefreshToken))
            return null;

        var storedToken = await _unitOfWork.RefreshTokens
            .GetByTokenHashAsync(_tokenService.HashToken(rawRefreshToken));

        if (storedToken is null || !storedToken.IsActive)
            return null; // نامعتبر، منقضی یا قبلاً باطل شده است

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

        return new TokenPair(
            newAccessToken,
            newRefreshToken,
            AccessTokenExpiresAtUtc: DateTime.UtcNow.AddMinutes(AccessTokenMinutes),
            RefreshTokenExpiresAtUtc: DateTime.UtcNow.AddMinutes(RefreshTokenMinutes));
    }

    // ═══════════ گام 22 | Refresh Tokens: خروج و ابطال ═══════════
    // JWT ذاتاً «بدون حالت» (Stateless) است؛ سرور نمی‌تواند توکن دسترسی را از راه دور
    // باطل کند و تا پایان عمرش (چند دقیقه) معتبر می‌ماند. آنچه در خروج واقعاً انجام می‌دهیم:
    // ابطال توکن تمدید در دیتابیس → بعد از انقضای توکن دسترسی، دیگر قابل تمدید نیست
    // و مسیر دسترسی کاربر به‌کلی بسته می‌شود.
    public async Task<bool> RevokeTokenAsync(string rawRefreshToken)
    {
        if (string.IsNullOrEmpty(rawRefreshToken))
            return false;

        var storedToken = await _unitOfWork.RefreshTokens
            .GetByTokenHashAsync(_tokenService.HashToken(rawRefreshToken));

        if (storedToken is not { IsActive: true })
            return false;

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
