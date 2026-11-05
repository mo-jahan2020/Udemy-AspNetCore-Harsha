namespace JwtDemoMVC.Models;

/// <summary>
/// موجودیت «توکن تمدید» (Refresh Token) — گام‌های 15 تا 22 پیاده‌سازی Refresh Token
/// روی همین کلاس و جدول متناظرش استوار است.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    // به دلایل امنیتی فقط «هش» توکن در دیتابیس ذخیره می‌شود، نه خود توکن (گام 17)
    public string TokenHash { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    // پایان اعتبار توکن تمدید (طبق تنظیمات: 60 دقیقه)
    public DateTime ExpiresAtUtc { get; set; }

    // اگر مقدار داشته باشد یعنی این توکن باطل شده است
    public DateTime? RevokedAtUtc { get; set; }

    // هش توکن جایگزین — برای نگه‌داری زنجیره‌ی چرخش توکن (گام 20)
    public string? ReplacedByTokenHash { get; set; }

    // توکن فعال است؟ (نه باطل شده و نه منقضی)
    public bool IsActive => RevokedAtUtc == null && ExpiresAtUtc > DateTime.UtcNow;
}
