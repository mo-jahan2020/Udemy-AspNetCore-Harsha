using JwtDemo.Models;

namespace JwtDemo.Services;

/// <summary>
/// قرارداد سرویس احراز هویت — هماهنگی توکن‌ها، کوکی‌ها و دیتابیس.
/// پیاده‌سازی گام‌های 17 تا 22 (Refresh Tokens) در کلاس AuthService است.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// صدور جفت توکن (دسترسی + تمدید)، ذخیره‌ی توکن تمدید در دیتابیس و
    /// تنظیم کوکی‌ها — بعد از ورود یا ثبت‌نام موفق (گام‌های 17 و 18).
    /// </summary>
    Task IssueTokensAsync(HttpContext context, User user);

    /// <summary>
    /// اگر توکن دسترسی منقضی شده باشد، با توکن تمدیدِ معتبر، جفت توکن جدید صادر می‌کند
    /// و مقدار آن را برمی‌گرداند؛ در غیر این صورت null (گام‌های 19 و 20).
    /// </summary>
    Task<string?> TryRefreshAsync(HttpContext context);

    /// <summary>
    /// ابطال توکن تمدید در دیتابیس و حذف کوکی‌ها — هنگام خروج (گام 22).
    /// </summary>
    Task RevokeAsync(HttpContext context);
}
