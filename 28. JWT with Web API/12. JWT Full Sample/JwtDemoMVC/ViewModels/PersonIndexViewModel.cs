using System.Security.Claims;
using JwtDemoMVC.Models;

namespace JwtDemoMVC.ViewModels;

/// <summary>
/// مدل نمایش صفحه‌ی اشخاص: جدول + فرم‌های افزودن/ویرایش + داده‌های پنل آموزشی JWT.
/// </summary>
public class PersonIndexViewModel
{
    // ───────── جدول اشخاص ─────────
    public IReadOnlyList<Person> People { get; set; } = Array.Empty<Person>();

    // شناسه‌ی شخصی که این‌بار در حالت ویرایش نمایش داده می‌شود (null = بدون ویرایش)
    public int? EditId { get; set; }

    // مقدار فرم افزودن/ویرایش (برای پر کردن Inputها)
    public Person Person { get; set; } = new();

    // ───────── پنل آموزشی JWT ─────────
    public string? AccessToken { get; set; }
    public string? TokenHeaderJson { get; set; }
    public string? TokenPayloadJson { get; set; }
    public DateTime? AccessExpiresAtUtc { get; set; }
    public IReadOnlyList<Claim> TokenClaims { get; set; } = Array.Empty<Claim>();
    public string? RefreshTokenMasked { get; set; }
    public DateTime? RefreshExpiresAtUtc { get; set; }

    // نام کاربریِ جاری از Claims توکن خوانده می‌شود (گام 14)
    public string CurrentUsername { get; set; } = "-";
}
