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
