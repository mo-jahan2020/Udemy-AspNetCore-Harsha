using JwtDemo.Models;

namespace JwtDemo.Services;

/// <summary>
/// قرارداد سرویس توکن — تمام منطق «ساخت» و «هش» توکن‌های JWT اینجاست.
/// (اینترفیس جزو پیاده‌سازی JWT نیست؛ کامنت‌های شماره‌دار در کلاس TokenService هستند)
/// </summary>
public interface ITokenService
{
    /// <summary>تولید توکن دسترسی JWT برای کاربر (گام‌های 4 تا 8)</summary>
    string GenerateAccessToken(User user);

    /// <summary>تولید مقدار تصادفی و امن برای توکن تمدید (گام 16)</summary>
    string GenerateRefreshToken();

    /// <summary>هش کردن توکن تمدید قبل از ذخیره در دیتابیس (گام 17)</summary>
    string HashToken(string token);
}
