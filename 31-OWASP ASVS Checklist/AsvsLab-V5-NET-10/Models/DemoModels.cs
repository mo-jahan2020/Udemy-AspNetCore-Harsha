using System.ComponentModel.DataAnnotations;

namespace AsvsLab.Models;

// ═══════════════════════════════════════════════════════════════════
//  مدل‌های نمایشی برنامه — نسخه ۲ (ASVS 5.0)
//  همه کلاس‌ها فقط برای انتقال داده بین کنترلر و ویو استفاده می‌شوند
// ═══════════════════════════════════════════════════════════════════

/// <summary>نتیجه یک دمو؛ شامل متن رسمی بند، توضیح فارسی، کد ناامن/امن و خروجی واقعی</summary>
public class DemoResult
{
    public string DemoId { get; set; } = "";            // شناسه یکتا برای ردیابی پیشرفت
    public string Title { get; set; } = "";             // عنوان فارسی دمو
    public string AsvsRef { get; set; } = "";           // شماره بند (مثلاً V11.4.2 یا V3.3.1, V3.3.2)
    public string AsvsSummaryEn { get; set; } = "";     // متن رسمی انگلیسی بند (از Data/asvs5.json)
    public string AsvsFa { get; set; } = "";            // توضیح فارسی بند
    public string? VulnerableCode { get; set; }         // کد ناامن (ستون ❌)
    public string? SecureCode { get; set; }             // کد امن (ستون ✅)
    public List<string> OutputLines { get; set; } = new(); // خروجی واقعی اجرا
    public bool Success { get; set; } = true;           // وضعیت اجرای دمو
    public string? ServiceNote { get; set; }            // نیازمندی سرویس خارجی (مثل Redis)
    public string? Notes { get; set; }                  // توضیح متنی تکمیلی (برای دموهای متنی)
    public string BackController { get; set; } = "Home";// برای دکمه بازگشت
}

/// <summary>مدل فرم ورودی کاربر برای دموهای تعاملی</summary>
public class DemoFormViewModel
{
    public string Title { get; set; } = "";
    public string AsvsRef { get; set; } = "";
    public string AsvsSummaryEn { get; set; } = "";
    public string AsvsFa { get; set; } = "";
    public string Action { get; set; } = "";            // مسیر اکشن POST
    public string FieldName { get; set; } = "";         // نام فیلد ورودی
    public string FieldLabel { get; set; } = "";        // برچسب فارسی
    public string Placeholder { get; set; } = "";
    public string Hint { get; set; } = "";              // راهنمای داخل فرم
    public bool IsUpload { get; set; }                  // آیا فرم آپلود فایل است؟
    public DemoResult? Result { get; set; }             // نتیجه پس از ارسال فرم
}

/// <summary>یک دمو در کاتالوگ — برای ساخت داشبورد، جستجو و ردیابی پیشرفت</summary>
public record DemoItem(string Id, int Chapter, string Title, string AsvsRefs,
                       string Controller, string Action, DemoKind Kind);

public enum DemoKind { Live, Form, Text }               // اجرایی / با فرم ورودی / متنی

/// <summary>مدل صفحه فهرست دموهای هر فصل</summary>
public class SectionIndexViewModel
{
    public int Chapter { get; set; }
    public string FaTitle { get; set; } = "";
    public string EnTitle { get; set; } = "";
    public string ChapterSummary { get; set; } = "";
    public List<DemoItem> Items { get; set; } = new();
    public int QuizScore { get; set; } = -1;            // -1 یعنی هنوز کوییز داده نشده
}

// ═══════════════════════════════════════════════════════════════════
//  مدل‌های احراز هویت واقعی (کنترلر Account)
// ═══════════════════════════════════════════════════════════════════

public class LoginViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "گذرواژه الزامی است")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? Error { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [RegularExpression(@"^[a-zA-Z0-9_]{3,30}$", ErrorMessage = "فقط حروف انگلیسی، رقم و زیرخط (۳ تا ۳۰ کاراکتر)")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "گذرواژه الزامی است")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public List<string> PolicyErrors { get; set; } = new();
}

/// <summary>مدل اعتبارسنجی شبیه‌سازی یک endpoint واقعی API (دموی فصل V4)</summary>
public class ApiOrderViewModel
{
    [Required(ErrorMessage = "product الزامی است")]
    public string? Product { get; set; }

    [Range(1, 100, ErrorMessage = "quantity باید بین ۱ تا ۱۰۰ باشد")]
    public int Quantity { get; set; }
}
