using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JwtDemoMVC.Models;

/// <summary>
/// موجودیت «IP مجاز» — آدرس‌هایی که اجازه‌ی استفاده از بخش‌های محافظت‌شده‌ی برنامه (JWT) را دارند.
/// این ویژگی مکمل JWT است، نه جایگزین آن: اول IP بررسی می‌شود (Middleware) و بعد توکن
/// (احراز هویت JWT + [Authorize]). یعنی «کدام رایانه» + «کدام کاربر» هر دو لازم‌اند.
/// </summary>
public class AllowedIp
{
    public int Id { get; set; }

    // نمایش استاندارد و نرمال‌شده‌ی IP (مثلاً ::ffff:127.0.0.1 به 127.0.0.1 تبدیل شده)
    // 45 = طول حداکثری نمایش متنی IPv6
    [MaxLength(45, ErrorMessage = "آدرس IP حداکثر 45 کاراکتر است")]
    public string IpAddress { get; set; } = string.Empty;

    
    // توضیح اختیاری — مثلاً «لپ‌تاپ دفتر» یا «سرور تست»
    [MaxLength(200, ErrorMessage = "توضیح حداکثر 200 کاراکتر است")]
    public string? Note { get; set; }

    
    public DateTime CreatedAtUtc { get; set; }

    // تاریخ اتمام اعتبار — بعد از این زمان Middleware دیگر اجازه‌ی عبور نمی‌دهد.
    // رکورد منقضی حذف نمی‌شود تا تاریخ و توضیحش در صفحه تنظیمات قابل مشاهده بماند.
    public DateTime ExpiresAtUtc { get; set; }

    // IP فعال است؟ (اعتبارش تمام نشده)
    public bool IsActive => DateTime.UtcNow <= ExpiresAtUtc;
}
