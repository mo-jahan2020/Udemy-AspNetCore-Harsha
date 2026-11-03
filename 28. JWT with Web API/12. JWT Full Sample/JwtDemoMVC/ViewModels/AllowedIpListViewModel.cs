using System.ComponentModel.DataAnnotations;

namespace JwtDemoMVC.ViewModels;

/// <summary>
/// مدل نمایش صفحه‌ی تنظیمات IPهای مجاز: جدول رکوردها + فرم افزودن + وضعیت IP فعلی کاربر.
/// </summary>
public class AllowedIpListViewModel
{
    // ───────── جدول IPهای مجاز ─────────
    public IReadOnlyList<Models.AllowedIp> Items { get; set; } = Array.Empty<Models.AllowedIp>();

    // ───────── وضعیت IP فعلی کاربر (بنر بالای جدول) ─────────
    public string CurrentIp { get; set; } = "-";
    public bool CurrentIpAllowed { get; set; }

    // ───────── فرم افزودن IP جدید ─────────

    [Required(ErrorMessage = "آدرس IP الزامی است")]
    [Display(Name = "آدرس IP")]
    public string? NewIp { get; set; }

    [Required(ErrorMessage = "مدت اعتبار الزامی است")]
    [Range(1, 5256000, ErrorMessage = "اعتبار باید بین ۱ دقیقه تا ۱۰ سال باشد")]
    [Display(Name = "مدت اعتبار (دقیقه)")]
    public int? ValidityMinutes { get; set; } = 60;

    [MaxLength(200, ErrorMessage = "توضیح حداکثر 200 کاراکتر است")]
    [Display(Name = "توضیح (اختیاری)")]
    public string? Note { get; set; }
}
