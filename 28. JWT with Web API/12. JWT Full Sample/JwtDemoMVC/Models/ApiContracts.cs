using System.ComponentModel.DataAnnotations;

namespace JwtDemoMVC.Models;

// ═══════════════════════════════════════════════════════════════════════════
//  قراردادهای API — مدل‌های ورودی درخواست‌های Swagger
//  (این فایل جزو پیاده‌سازی JWT نیست؛ فقط حامل توکن‌ها در پاسخ API است)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>درخواست ثبت‌نام از طریق API</summary>
public class RegisterRequest
{
    /// <summary>نام کاربری (3 تا 50 کاراکتر)</summary>
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [MinLength(3, ErrorMessage = "نام کاربری باید حداقل 3 کاراکتر باشد")]
    [MaxLength(50, ErrorMessage = "نام کاربری حداکثر 50 کاراکتر است")]
    public string Username { get; set; } = string.Empty;

    /// <summary>رمز عبور (حداقل 4 کاراکتر)</summary>
    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [MinLength(4, ErrorMessage = "رمز عبور باید حداقل 4 کاراکتر باشد")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>درخواست ورود از طریق API</summary>
public class LoginRequest
{
    /// <summary>نام کاربری</summary>
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    public string Username { get; set; } = string.Empty;

    /// <summary>رمز عبور</summary>
    [Required(ErrorMessage = "رمز عبور الزامی است")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>درخواست تمدید یا ابطال توکن از طریق API</summary>
public class RefreshRequest
{
    /// <summary>مقدار خام توکن تمدید (همان refreshToken در پاسخ ورود)</summary>
    [Required(ErrorMessage = "توکن تمدید الزامی است")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>درخواست ابزار آموزشی بررسی توکن — POST /api/debug/token-check</summary>
public class TokenCheckRequest
{
    /// <summary>رشته‌ی کامل JWT (سه‌بخشی: Header.Payload.Signature)</summary>
    [Required(ErrorMessage = "توکن الزامی است")]
    public string Token { get; set; } = string.Empty;
}

// ═══ قراردادهای API تنظیمات IPهای مجاز — api/settings ═══
// نکته‌ی امنیتی: به‌جای Bind مستقیم موجودیت AllowedIp، از DTO استفاده می‌کنیم تا
// کلاینت نتواند فیلدهایی مثل Id یا CreatedAtUtc را از طریق بدنه‌ی درخواست تزریق کند (Over-posting).

/// <summary>درخواست ثبت IP مجاز — POST /api/settings</summary>
public class AllowedIpCreateRequest
{
    /// <summary>آدرس IP (مثال: 192.168.1.10 یا ::1) — نرمال‌سازی خودکار می‌شود</summary>
    [Required(ErrorMessage = "آدرس IP الزامی است")]
    [MaxLength(45, ErrorMessage = "آدرس IP حداکثر 45 کاراکتر است")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>مدت اعتبار به دقیقه (پیش‌فرض 60 — حداکثر 5,256,000 دقیقه یعنی ۱۰ سال)</summary>
    [Range(1, 5_256_000, ErrorMessage = "مدت اعتبار باید بین ۱ دقیقه تا ۱۰ سال باشد")]
    public int? ValidityMinutes { get; set; } = 60;

    /// <summary>توضیح اختیاری (مثلاً «لپ‌تاپ دفتر»)</summary>
    [MaxLength(200, ErrorMessage = "توضیح حداکثر 200 کاراکتر است")]
    public string? Note { get; set; }
}

/// <summary>درخواست ویرایش IP مجاز — PUT /api/settings/{id}</summary>
public class AllowedIpUpdateRequest
{
    /// <summary>آدرس IP جدید (نرمال‌سازی خودکار می‌شود)</summary>
    [Required(ErrorMessage = "آدرس IP الزامی است")]
    [MaxLength(45, ErrorMessage = "آدرس IP حداکثر 45 کاراکتر است")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>مدت اعتبار جدید به دقیقه — از همین لحظه محاسبه می‌شود</summary>
    [Range(1, 5_256_000, ErrorMessage = "مدت اعتبار باید بین ۱ دقیقه تا ۱۰ سال باشد")]
    public int? ValidityMinutes { get; set; } = 60;

    /// <summary>توضیح اختیاری</summary>
    [MaxLength(200, ErrorMessage = "توضیح حداکثر 200 کاراکتر است")]
    public string? Note { get; set; }
}
