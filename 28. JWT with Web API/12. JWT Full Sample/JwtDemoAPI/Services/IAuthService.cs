using JwtDemoMVC.Models;

namespace JwtDemoMVC.Services;

/// <summary>
/// جفت توکن صادرشده برای کاربر — API این شیء را به‌صورت JSON به کلاینت برمی‌گرداند
/// (همان چیزی که در Swagger می‌بینید).
/// </summary>
/// <param name="AccessToken">توکن دسترسی JWT (عمر کوتاه)</param>
/// <param name="RefreshToken">توکن تمدید (عمر 60 دقیقه)</param>
/// <param name="AccessTokenExpiresAtUtc">زمان انقضای توکن دسترسی</param>
/// <param name="RefreshTokenExpiresAtUtc">زمان انقضای توکن تمدید</param>
public record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);

/// <summary>
/// قرارداد سرویس احراز هویت — هماهنگی توکن‌ها و دیتابیس (نسخه‌ی Web API خالص).
/// پیاده‌سازی گام‌های 17 و 19 و 20 و 22 (Refresh Tokens) در کلاس AuthService است.
/// همه‌ی متدها «خام» (بدون کوکی) هستند؛ کلاینت API توکن‌ها را در پاسخ JSON
/// می‌گیرد، خودش نگه می‌دارد و برای تمدید/خروج در بدنه‌ی درخواست می‌فرستد.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// صدور جفت توکن و ذخیره‌ی توکن تمدید در دیتابیس — بدون کوکی؛
    /// برای کلاینت‌های API و Swagger که توکن را در پاسخ JSON می‌گیرند (گام 17).
    /// </summary>
    Task<TokenPair> IssueTokensForApiAsync(User user);

    /// <summary>
    /// اعتبارسنجی و چرخش توکن تمدید خام — برای API (گام‌های 19 و 20).
    /// </summary>
    Task<TokenPair?> RefreshTokenAsync(string rawRefreshToken);

    /// <summary>
    /// ابطال توکن تمدید خام در دیتابیس — خروج از API (گام 22).
    /// </summary>
    Task<bool> RevokeTokenAsync(string rawRefreshToken);
}
