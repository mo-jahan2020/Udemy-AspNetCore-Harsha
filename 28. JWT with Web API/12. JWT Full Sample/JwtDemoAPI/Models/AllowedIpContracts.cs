using System.ComponentModel.DataAnnotations;

namespace JwtDemoMVC.Models;

/// <summary>
/// ورودی Create/Update برای IP مجاز.
/// چرا DTO و نه خودِ Entity؟
///   • ضد Over-posting: کلاینت نمی‌تواند Id، CreatedAtUtc یا ExpiresAtUtc را مقداردهی کند.
///   • قرارداد API از مدل دیتابیس جدا می‌ماند (تغییر Entity، قرارداد Swagger را نمی‌شکند).
///   • اعتبارسنجی ورودی در یک جا متمرکز می‌شود.
/// </summary>
public class AllowedIpRequest
{
    /// <summary>آدرس IP — IPv4 کامل (192.168.1.10) یا IPv6 (::1)</summary>
    [Required(ErrorMessage = "آدرس IP الزامی است")]
    [MaxLength(45, ErrorMessage = "آدرس IP حداکثر 45 کاراکتر است")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>توضیح اختیاری (مثلاً «لپ‌تاپ دفتر»)</summary>
    [MaxLength(200, ErrorMessage = "توضیح حداکثر 200 کاراکتر است")]
    public string? Note { get; set; }

    /// <summary>
    /// مدت اعتبار به دقیقه. در Create خالی یعنی پیش‌فرضِ یک سال؛
    /// در Update خالی یعنی «تاریخ انقضای فعلی دست نخورد» (فقط IP/توضیح عوض می‌شود).
    /// </summary>
    [Range(1, 5_256_000, ErrorMessage = "مدت اعتبار باید بین ۱ دقیقه تا ۱۰ سال باشد")]
    public int? ValidityMinutes { get; set; }
}

/// <summary>
/// خروجی API برای یک IP مجاز — فقط فیلدهای قابل نمایش.
/// </summary>
public class AllowedIpResponse
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; }

    public static AllowedIpResponse FromEntity(AllowedIp entity) => new()
    {
        Id = entity.Id,
        IpAddress = entity.IpAddress,
        Note = entity.Note,
        CreatedAtUtc = entity.CreatedAtUtc,
        ExpiresAtUtc = entity.ExpiresAtUtc,
        IsActive = entity.IsActive,
    };
}