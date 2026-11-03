using JwtDemoMVC.Models;

namespace JwtDemoMVC.Services;

/// <summary>
/// جفت توکن صادرشده برای کاربر — API این شیء را به‌صورت JSON به کلاینت برمی‌گرداند
/// (همان چیزی که در Swagger می‌بینید).
/// </summary>
/// <param name="AccessToken">توکن دسترسی JWT (عمر 1 دقیقه)</param>
/// <param name="RefreshToken">توکن تمدید (عمر 60 دقیقه)</param>
/// <param name="AccessTokenExpiresAtUtc">زمان انقضای توکن دسترسی</param>
/// <param name="RefreshTokenExpiresAtUtc">زمان انقضای توکن تمدید</param>
public record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);

/// <summary>
/// قرارداد سرویس احراز هویت — هماهنگی توکن‌ها، کوکی‌ها و دیتابیس.
/// پیاده‌سازی گام‌های 17 تا 22 (Refresh Tokens) در کلاس AuthService است.
/// دو گروه متد دارد: «کوکی‌محور» برای رابط کاربری MVC و «خام» برای کلاینت‌های API/Swagger.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// صدور جفت توکن، ذخیره‌ی توکن تمدید در دیتابیس و تنظیم کوکی‌ها —
    /// بعد از ورود/ثبت‌نام در رابط کاربری MVC (گام‌های 17 و 18).
    /// </summary>
    Task IssueTokensAsync(HttpContext context, User user);

    /// <summary>
    /// صدور جفت توکن و ذخیره‌ی توکن تمدید در دیتابیس — بدون کوکی؛
    /// برای کلاینت‌های API و Swagger که توکن را در پاسخ JSON می‌گیرند (گام 17).
    /// </summary>
    Task<TokenPair> IssueTokensForApiAsync(User user);

    /// <summary>
    /// تمدید خودکار از روی کوکی‌ها — اگر توکن دسترسی منقضی شده باشد، جفت توکن جدید
    /// صادر و کوکی‌ها تنظیم می‌شوند؛ خروجی، توکن دسترسی جدید است (گام‌های 19، 20 و 21).
    /// </summary>
    Task<string?> TryRefreshAsync(HttpContext context);

    /// <summary>
    /// اعتبارسنجی و چرخش توکن تمدید خام — برای API (گام‌های 19 و 20).
    /// </summary>
    Task<TokenPair?> RefreshTokenAsync(string rawRefreshToken);

    /// <summary>
    /// خروج رابط کاربری: ابطال توکن تمدید کوکی و حذف کوکی‌ها (گام 22).
    /// </summary>
    Task RevokeAsync(HttpContext context);

    /// <summary>
    /// ابطال توکن تمدید خام در دیتابیس — خروج از API (گام 22).
    /// </summary>
    Task<bool> RevokeTokenAsync(string rawRefreshToken);
}
